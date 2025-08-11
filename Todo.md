# Toilet Ghost

This is a web app where musicians can share works in progress for songs. Toilet Ghost was a joke that kind of ended up being the mascot for this app. You can make tounge n cheek jokes within the app. I appreciate the funny. 

## Work to be done

As items get completed update this document and next to the feature put "- DONE". Summarize the work that was done underneath the feature. If I mention screenshots and I forget to paste one in please ask me for one.

### Project setup
* Blazor web server
  * Project started
  * Add robots.txt and other headers to prevent the site from being crawled.
* Azure Storage
  * Add placeholders in #appsetting.json for the connection string and blob container.
  * The container will be private. When a user requests a file you should use the presigned methods in the Blob client. You can set the lease for 24 hours. For project versions, allow the user to share this link with the lease on it. 
    * I'll add the proper values in my dotnet secrets.
  * I want to use azure blob storage for files.
    * Storage of the files
      * When a user uploads an audio file rename the file with this format "timestamp-member-project-version"
      * be sure to make the filenames sanitized and remove any spaces or unsafe characters
      * Acceptable types for audio
        * wav
        * mp3
        * m4a
        * aiff
        * ogg
      * Acceptable types for images (only allowed in comments and artwork for a project)
        * png
        * jpg
        * gif (under 2mb)
    * Size restrictions
        * Audio 75 mb
        * Images 2 mb
  * I want to use azure data-tables for the database
    * no need to use ef framework, just write data straight to the tabes. Use best practices for sanitizing any input.


### Data Layout
Add constrains and foreign keys where you see fit.
Here's the structure for the data:
* User
    * email - required user cannot change
    * nickname - Let user change this in UI - Initial value Ghost#[some random value]
    * Projects - List of projects also referred to as songs
        * name - name of song
        * artwork - an image for artwork
        * Versions - A list 
            * filename - filename in azure storage (75mb file size limit)
            * version number - this can be a time stamp and sort by this so the newest version is on top
        * Comments
            * nickname - person who made the comment
            * type
                * image - store the file in blob storage and keep the file name in the commentData field. When you show the image back in the comments get a presigned URL from the azure blob client
                * text - store the message in commentData
                * url - store the link in commentData. In the UI show the URL and pull in the social preview underneath it. When a user clicks on the link be sure to warn them before leaving the site.
            * commentData - String to hold data to show

### Site

I have a library called arb.js installed in #wwwroot\arb.js. It mimicks the style of using arbitrary values like tailwind css.
We're gearing towards a mobile first design. The color scheme should be purple but not hard on the eyes. make sure it's readable. Use emojis for everything! Keep to a flat material style when making design choices.  

I want the site to feel like a windows file explorer.

Site icon should be in top left. An emoji for the user can be on the right. There they can have a button they can click to open a modal to update their nickname. There should be a button to create a new project there too.

Under that should be a tree structiure for navigation. The tree should look like the following
* nickname - sort alphabetically
 * project (number of versions) - sort this by the latest update project

The detail side when a user is clicked can show the artwork for all the projects for that user. When a user picks a project it should collaps the tree and show the project view

 #### Project View
 The project view should have a media player at the top. Use this example from wavesurfer.xyw I already have the UMD tag in #app.razor
 ```html

// Web Audio example

import WaveSurfer from 'wavesurfer.js'

// Define the equalizer bands
const eqBands = [32, 64, 125, 250, 500, 1000, 2000, 4000, 8000, 16000]

// Create a WaveSurfer instance and pass the media element
const wavesurfer = WaveSurfer.create({
  container: document.body,
  waveColor: 'rgb(200, 0, 200)',
  progressColor: 'rgb(100, 0, 100)',
  url: '/examples/audio/audio.wav',
  mediaControls: true,
})

wavesurfer.on('click', () => wavesurfer.playPause())

wavesurfer.once('play', () => {
  // Create Web Audio context
  const audioContext = new AudioContext()

  // Create a biquad filter for each band
  const filters = eqBands.map((band) => {
    const filter = audioContext.createBiquadFilter()
    filter.type = band <= 32 ? 'lowshelf' : band >= 16000 ? 'highshelf' : 'peaking'
    filter.gain.value = Math.random() * 40 - 20
    filter.Q.value = 1 // resonance
    filter.frequency.value = band // the cut-off frequency
    return filter
  })

  const audio = wavesurfer.getMediaElement()
  const mediaNode = audioContext.createMediaElementSource(audio)

  // Connect the filters and media node sequentially
  const equalizer = filters.reduce((prev, curr) => {
    prev.connect(curr)
    return curr
  }, mediaNode)

  // Connect the filters to the audio output
  equalizer.connect(audioContext.destination)

  sliders.forEach((slider, i) => {
    const filter = filters[i]
    filter.gain.value = slider.value
    slider.oninput = (e) => (filter.gain.value = e.target.value)
  })
})

// HTML UI
// Create a vertical slider for each band
const container = document.createElement('p')
const sliders = eqBands.map(() => {
  const slider = document.createElement('input')
  slider.type = 'range'
  slider.orient = 'vertical'
  slider.style.appearance = 'slider-vertical'
  slider.style.width = '8%'
  slider.min = -40
  slider.max = 40
  slider.value = Math.random() * 40 - 20
  slider.step = 0.1
  container.appendChild(slider)
  return slider
})
document.body.appendChild(container)

 ```
This should load the latest version of the project. Include the artwork somewhere in that player.

If no versions are avaiable provide a button to create a new version.

If the user is the owner of the project they can, see a button to upload a new version.

Under that all users can add comments. We'll allow for 3 types.

* Add Image
    * Let a user upload an image comment
* Add Comment
    * Just a text comment. be sure to sanitize the input
* Add Link
    * Add a link and when a user clicks it, show a modal to confirm they want to leave.


#### arb.js: Arbitrary CSS Utility Class System

This script enables you to define CSS styles directly in your HTML class attributes using a special format. It supports arbitrary CSS properties and values, as well as media queries for responsive design.

## Usage

### 1. Arbitrary CSS Classes

To apply a CSS property/value pair, use the following class format:

```
property-[value]
```

- `property`: The CSS property name (in camelCase or kebab-case).
- `value`: The value for the property.

**Examples:**

```html
<div class="color-[red]"></div>               <!-- Sets color: red -->
<div class="background-color-[#333]"></div>   <!-- Sets background-color: #333 -->
<div class="font-size-[2rem]"></div>          <!-- Sets font-size: 2rem -->
```

### 2. Media Queries (Responsive Styles)

To apply styles at specific breakpoints, prefix the class with the breakpoint name and a colon:

```
breakpoint:property-[value]
```

- `breakpoint`: One of the supported breakpoints (see below).
- `property`: The CSS property.
- `value`: The value for the property.

**Examples:**

```html
<div class="md:font-size-[2rem]"></div>   <!-- font-size: 2rem at min-width: 768px -->
<div class="lg:color-[blue]"></div>       <!-- color: blue at min-width: 1024px -->
```

#### Supported Breakpoints

The default breakpoints are:

| Prefix | Min Width |
|--------|-----------|
| sm     | 640px     |
| md     | 768px     |
| lg     | 1024px    |
| xl     | 1280px    |
| 2xl    | 1536px    |

You can use these as prefixes for responsive classes.

### 3. Multiple Classes

You can combine as many utility classes as you want:

```html
<div class="color-[red] md:color-[blue] font-size-[1.5rem]"></div>
```

### 4. Special Notes

- The script automatically generates and injects the required CSS into the page.
- Only classes matching the `[property]-[value]` or `breakpoint:[property]-[value]` format are processed.
- You can use any valid CSS property and value.

---

## Example

```html
<div class="background-color-[#222] color-[white] md:padding-[2rem] lg:font-size-[2rem]">
  Responsive styled box
</div>
```

- On all screens: `background-color: #222; color: white;`
- On screens ≥768px: `padding: 2rem;`
- On screens ≥1024px: `font-size: 2rem;`