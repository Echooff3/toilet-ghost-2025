const sanitizeCSSName = (input) => {
    let result = '';
    const regex = /^[a-zA-Z0-9]$/; // Regular expression to check for numbers and letters
    for (let i = 0; i < input.length; i++) {
        let c = input[i];
        if (regex.test(c)) {
            result += c;
        } else {
            result += '\\' + c;
        }
    }
    return result;
}

const breakpoints = {
    sm: 576,
    md: 768,
    lg: 992,
    xl: 1200,
    xxl: 1400,
}

const bpTemplate = `\n@media screen and (min-width: _px) {\n\t__\n}\n`

const lookup = {
    w: "width",
    h: "height",
    m: "margin",
    mt: "margin-top",
    mb: "margin-bottom",
    ml: "margin-left",
    mr: "margin-right",
    mx: "margin-left|margin-right",
    my: "margin-top|margin-bottom",
    p: "padding",
    pt: "padding-top",
    pb: "padding-bottom",
    pl: "padding-left",
    pr: "padding-right",
    px: "padding-left|padding-right",
    py: "padding-top|padding-bottom",
    t: "top",
    r: "right",
    b: "bottom",
    l: "left",
    fs: "font-size",
    fw: "font-weight",
    ff: "font-family",
    bg: "background",
    c: "color",
    b: "border",
    bl: "border-left",
    br: "border-right",
    bt: "border-top",
    bb: "border-bottom",
    radius: "border-radius",
    d: "display",
    jc: "justify-content",
    ai: "align-items",
    gap: "gap",
    o: "overflow",
    of: "overflow",
    pos: "position",
    z: "z-index",
    aspect: "aspect-ratio",
    "grid-cols": "grid-template-columns",
    "grid-rows": "grid-template-rows",
    "max-w": "max-width",
    "max-h": "max-height",
    "min-w": "min-width",
    "min-h": "min-height",
    fs: "font-size",
    bg: "background",
    lh: "line-height",
}

// Function to generate a random 8-character string
const generateRandomID = () => {
    return Math.random().toString(36).substring(2, 10);
}

// Define the xUpgrade function
const xUpgrade = (newName, item, first, second, element) => {
    // Remove the 'x__' prefix and save it in a variable
    const originalItem = item.replace(/^x__/, '');
    first = first.replace(/^x__/, '');
    // Extract a CSS selector for the element
    let selector;
    if (element.id) {
        selector = `#${element.id}`;
    } else {
        const randomID = generateRandomID();
        element.id = randomID;
        selector = `#${randomID}`;
    }

    console.log(`Handling special case for ${originalItem} on element: ${selector}`);
    // Add your special case logic here

    // Example: Return a modified rule
    return `.${newName} > * { ${first}: ${second}; /* special case handled */ }\n`;
}

const ruleExists = (rule) => {
    for (let i = 0; i < document.styleSheets.length; i++) {
        const sheet = document.styleSheets[i];
        try {
            const rules = sheet.cssRules || sheet.rules;
            for (let j = 0; j < rules.length; j++) {
                if (rules[j].cssText === rule) {
                    return true;
                }
            }
        } catch (e) {
            // Ignore errors from cross-origin stylesheets
        }
    }
    return false;
}

const __arb = _ => {
    console.log('scraping')

    let style = document.getElementById('arb');
    if (!style) {
        style = document.createElement('style');
        style.id = 'arb';
        style.type = 'text/css';
        document.head.appendChild(style);
    }
    style.innerHTML = '';

    let classes = []
    // grab every element class in the document
    document.querySelectorAll('*').forEach(element => {
        // if the element has a class
        if (element.className) {
            // split the class into an array
            let classesFound = String(element.className).split(' ');
            // loop through the array
            classesFound.forEach(cls => {
                // if the class is not empty
                if (cls !== '') {
                    classes.push({ cls, element });
                }
            });
        }
    });

    // remove any classes that do not contain []
    classes = classes.filter(el => el.cls.includes('[') && el.cls != "[object" && el.tagName != 'svg');
    // remove any duplicates
    classes = [...new Set(classes.map(el => el.cls))].map(cls => {
        return classes.find(el => el.cls === cls);
    });

    // Create a set to keep track of added styles
    const addedStyles = new Set();

    // for each class in classes split out the values from first-[second]
    classes.forEach(({ cls, element }) => {
        let split = cls.split('[');
        let first = split[0].replace(/-$/, '').substring(split[0].indexOf(":") + 1, split[0].length).substring(split[0].indexOf("*") + 1, split[0].length);
        let second = split[1].replaceAll(']', '').replaceAll('[', '').replaceAll('_', ' ');
        // lookup the first value in the lookup object
        first = lookup[first] || first;
        // escape item so it is a valid css name
        let newName = sanitizeCSSName(cls);

        let rule = `.${newName} { ${first}: ${second};}\n`

        if (first.indexOf("|") >= 0) {
            let [first1, first2] = first.split('|')
            rule = `.${newName} { ${first1}: ${second}; ${first2}: ${second};}\n`
        } else {
            rule = `.${newName} { ${first}: ${second};}\n`
        }
        const psuedos = ["hover", "focus", "active", "visited", "disabled", "checked", "required", "valid", "invalid", "optional", "in-range", "out-of-range", "read-only", "read-write", "placeholder-shown", "default", "indeterminate", "blank", "nth-child", "nth-last-child", "first-child", "last-child", "first-of-type", "last-of-type", "only-child", "only-of-type", "root", "empty", "target", "enabled", "disabled", "fullscreen", "selection", "before", "after", "first-letter", "first-line", "backdrop", "grammar-error", "spelling-error", "marker", "placeholder", "user-invalid", "not", "nth-match", "nth-last-match", "current", "past", "future", "active", "any-link", "blank", "current", "default", "dir", "drop", "empty", "enabled", "first", "first-child", "first-letter", "first-line", "first-of-type", "fullscreen", "host", "host-context", "indeterminate", "in-range", "invalid", "lang", "last-child", "last-of-type", "left", "link", "not", "nth-child", "nth-col", "nth-last-child", "nth-last-col", "nth-last-of-type", "nth-of-type", "only-child", "only-of-type", "optional", "out-of-range", "past", "placeholder", "placeholder-shown", "read-only", "read-write", "required", "right", "root", "scope", "target", "valid", "visited"]
        if (psuedos.findIndex(x => cls.startsWith(`${x}:`)) > -1) {
            let pseudo = cls.split(':');
            rule = `.${newName}:${pseudo[0]} { ${first}: ${second}!important; }`;
        }

        if (newName.indexOf(`*`) > 0) {
            rule = bpTemplate.replace('_', breakpoints[newName.split('\\*')[0]]).replace('__', rule)
        }

        // Check for special cases and call xUpgrade if needed
        if (first.startsWith('x')) {
            rule = xUpgrade(newName, cls, first, second, element);
        }



        // Check if the rule has already been added or exists in the document
        if (!addedStyles.has(rule) && !ruleExists(rule)) {
            style.innerHTML += rule;
            addedStyles.add(rule);
        }
    });
    document.head.appendChild(style);
}


document.addEventListener('keyup', (event) => {
    if (event.shiftKey && event.key === 'a') {
        __arb()
    }
});

document.addEventListener('DOMContentLoaded', __arb);

// WaveSurfer initialization function
window.initializeWaveSurfer = (projectId, audioUrl) => {
    import('https://unpkg.com/wavesurfer.js@7').then(({ default: WaveSurfer }) => {
        const containerId = `waveform-${projectId}`;
        const playBtnId = `play-btn-${projectId}`;
        
        const container = document.getElementById(containerId);
        const playBtn = document.getElementById(playBtnId);
        
        if (!container || !playBtn) {
            console.error('Container or play button not found');
            return;
        }

        // Create WaveSurfer instance
        const wavesurfer = WaveSurfer.create({
            container: container,
            waveColor: '#8b5a96',
            progressColor: '#ffffff',
            cursorColor: '#ffffff',
            barWidth: 2,
            barRadius: 3,
            responsive: true,
            height: 100,
            normalize: true,
            backend: 'WebAudio',
            mediaControls: false,
        });

        // Load the audio
        wavesurfer.load(audioUrl);

        // Play/pause functionality
        let isPlaying = false;
        playBtn.addEventListener('click', () => {
            if (isPlaying) {
                wavesurfer.pause();
                playBtn.textContent = '▶️ Play';
                isPlaying = false;
            } else {
                wavesurfer.play();
                playBtn.textContent = '⏸️ Pause';
                isPlaying = true;
            }
        });

        // Update button when playback ends
        wavesurfer.on('finish', () => {
            playBtn.textContent = '▶️ Play';
            isPlaying = false;
        });

        // Store reference for cleanup
        window[`wavesurfer_${projectId}`] = wavesurfer;
    });
};