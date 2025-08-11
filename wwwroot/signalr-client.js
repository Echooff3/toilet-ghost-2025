// SignalR connection for real-time updates
let projectConnection = null;
let currentProjectId = null;
let projectComponent = null;

async function setupProjectSignalR(projectId, dotNetComponent) {
    try {
        console.log(`Setting up SignalR for project: ${projectId}`);
        
        // Store references
        currentProjectId = projectId;
        projectComponent = dotNetComponent;
        
        // Create connection if it doesn't exist
        if (!projectConnection) {
            projectConnection = new signalR.HubConnectionBuilder()
                .withUrl("/projecthub")
                .build();

            // Set up event handlers
            projectConnection.on("ProjectUpdated", async (project) => {
                console.log("Received ProjectUpdated signal", project);
                if (projectComponent && project.projectId === currentProjectId) {
                    await projectComponent.invokeMethodAsync('OnProjectUpdated', project);
                }
            });

            projectConnection.on("CommentAdded", async (comment) => {
                console.log("Received CommentAdded signal", comment);
                if (projectComponent && comment.projectId === currentProjectId) {
                    await projectComponent.invokeMethodAsync('OnCommentAdded', comment);
                }
            });

            projectConnection.on("VersionAdded", async (version) => {
                console.log("Received VersionAdded signal", version);
                if (projectComponent && version.projectId === currentProjectId) {
                    await projectComponent.invokeMethodAsync('OnVersionAdded', version);
                }
            });

            // Start the connection
            await projectConnection.start();
            console.log("SignalR connection started");
        }

        // Join the project group
        if (projectConnection.state === signalR.HubConnectionState.Connected) {
            await projectConnection.invoke("JoinProjectGroup", projectId);
            console.log(`Joined project group: ${projectId}`);
        }

    } catch (error) {
        console.error("SignalR setup failed:", error);
    }
}

// Clean up SignalR connection when leaving
function cleanupProjectSignalR() {
    if (projectConnection && currentProjectId) {
        projectConnection.invoke("LeaveProjectGroup", currentProjectId)
            .catch(err => console.error("Failed to leave project group:", err));
    }
    currentProjectId = null;
    projectComponent = null;
}

// Function to leave a specific project group (used during navigation)
async function leaveProjectGroup(projectId) {
    try {
        if (projectConnection && projectConnection.state === signalR.HubConnectionState.Connected) {
            await projectConnection.invoke("LeaveProjectGroup", projectId);
            console.log(`Left project group: ${projectId}`);
        }
    } catch (error) {
        console.error("Failed to leave project group:", error);
    }
}

// Global SignalR connection for general updates (like project list)
let globalConnection = null;
let homeComponent = null;
let treeComponent = null;

async function setupGlobalSignalR(dotNetComponent) {
    try {
        console.log("Setting up global SignalR");
        
        homeComponent = dotNetComponent;
        
        if (!globalConnection) {
            globalConnection = new signalR.HubConnectionBuilder()
                .withUrl("/projecthub")
                .build();

            globalConnection.on("ProjectListUpdated", async () => {
                console.log("Received ProjectListUpdated signal");
                if (homeComponent) {
                    await homeComponent.invokeMethodAsync('OnProjectListUpdated');
                }
                if (treeComponent) {
                    await treeComponent.invokeMethodAsync('OnProjectListUpdated');
                }
            });

            globalConnection.on("ProjectCreated", async (project) => {
                console.log("Received ProjectCreated signal", project);
                if (homeComponent) {
                    await homeComponent.invokeMethodAsync('OnProjectListUpdated');
                }
                if (treeComponent) {
                    await treeComponent.invokeMethodAsync('OnProjectListUpdated');
                }
            });

            globalConnection.on("ProjectUpdated", async (project) => {
                console.log("Received ProjectUpdated signal for global", project);
                if (homeComponent) {
                    await homeComponent.invokeMethodAsync('OnProjectListUpdated');
                }
                if (treeComponent) {
                    await treeComponent.invokeMethodAsync('OnProjectUpdated', project);
                }
            });

            await globalConnection.start();
            console.log("Global SignalR connection started");
        }

    } catch (error) {
        console.error("Global SignalR setup failed:", error);
    }
}

// Tree SignalR setup for sidebar project tree
async function setupTreeSignalR(dotNetComponent) {
    try {
        console.log("Setting up tree SignalR");
        
        treeComponent = dotNetComponent;
        
        // Use the same global connection
        await setupGlobalSignalR(null);

    } catch (error) {
        console.error("Tree SignalR setup failed:", error);
    }
}

// Join user group for personalized notifications
async function joinUserGroup(userEmail) {
    try {
        console.log(`Joining user group: ${userEmail}`);
        
        // Ensure global connection is available
        await setupGlobalSignalR(null);
        
        if (globalConnection && globalConnection.state === signalR.HubConnectionState.Connected) {
            await globalConnection.invoke("JoinUserGroup", userEmail);
            console.log(`Successfully joined user group: ${userEmail}`);
        } else {
            console.warn("Global connection not available for joining user group");
        }

    } catch (error) {
        console.error("Failed to join user group:", error);
    }
}
