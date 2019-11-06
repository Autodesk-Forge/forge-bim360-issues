# View and create BIM 360 Issues

![Platforms](https://img.shields.io/badge/platform-Windows|MacOS-lightgray.svg)
![.NET](https://img.shields.io/badge/.NET%20Core-2.1-blue.svg)
[![License](http://img.shields.io/:license-MIT-blue.svg)](http://opensource.org/licenses/MIT)

[![oAuth2](https://img.shields.io/badge/oAuth2-v1-green.svg)](http://developer.autodesk.com/)
[![Data-Management](https://img.shields.io/badge/Data%20Management-v1-green.svg)](http://developer.autodesk.com/)
[![Viewer](https://img.shields.io/badge/Viewer-v6-green.svg)](http://developer.autodesk.com/)
[![BIM-360](https://img.shields.io/badge/BIM%20360-v1-green.svg)](http://developer.autodesk.com/)

![Intermediate](https://img.shields.io/badge/Level-Intermediate-blue.svg)

# Description

Demonstrate how to read and create BIM 360 **Quality Issues** using built-in PushPin Viewer extension.

Uses [Data Management](https://developer.autodesk.com/en/docs/data/v2) to list hubs, projects and files. Uses [Viewer](https://developer.autodesk.com/en/docs/viewer/v6/overview/) to show models and extensions to create toolbar buttons and panels. The lists [BIM 360](https://developer.autodesk.com/en/docs/bim360/v1/overview/) Document Issues on the panel. Uses the `Autodesk.BIM360.Extension.PushPin` built-in extension to show pins on the model. 

This sample implements the [Retrieve Container ID](https://forge.autodesk.com/en/docs/bim360/v1/tutorials/issues/retrieve-container-id/), [Retrieve Issues](https://developer.autodesk.com/en/docs/bim360/v1/tutorials/retrieve-issues/) and [Create Issues](https://forge.autodesk.com/en/docs/bim360/v1/tutorials/issues/create-issues/) tutorials on server-side (C#) and [Render Pushpin](https://forge.autodesk.com/en/docs/bim360/v1/tutorials/pushpins/retrieve-pushpin/) and [Create Pushpin](https://forge.autodesk.com/en/docs/bim360/v1/tutorials/pushpins/create-pushpin/) tutorials on the client-side (JavaScript).

## Thumbnail

![thumbnail](/thumbnail.gif)

## Live version

[bim360issues.herokuapp.com](https://bim360issues.herokuapp.com/)

# Setup

## Prerequisites

1. **BIM 360 Account**: must be Account Admin to add the app integration. [Learn about provisioning](https://forge.autodesk.com/blog/bim-360-docs-provisioning-forge-apps).
2. **Forge Account**: Learn how to create a Forge Account, activate subscription and create an app at [this tutorial](http://learnforge.autodesk.io/#/account/). 
3. **Visual Studio**: Either Community 2017+ (Windows) or Code (Windows, MacOS).
4. **.NET Core** basic knowledge with C#
5. **JavaScript** basic knowledge with **jQuery**

## Running locally

Clone this project or download it. It's recommended to install [GitHub desktop](https://desktop.github.com/). To clone it via command line, use the following (**Terminal** on MacOSX/Linux, **Git Shell** on Windows):

    git clone https://github.com/autodesk-forge/bim360-csharp-issues

**Visual Studio** (Windows):

Right-click on the project, then go to **Debug**. Adjust the settings as shown below. 

![](bim360issues/wwwroot/img/readme/visual_studio_settings.png) 

**Visual Sutdio Code** (Windows, MacOS):

Open the folder, at the bottom-right, select **Yes** and **Restore**. This restores the packages (e.g. Autodesk.Forge) and creates the launch.json file. See *Tips & Tricks* for .NET Core on MacOS.

![](bim360issues/wwwroot/img/readme/visual_code_restore.png)

At the `.vscode\launch.json`, find the env vars and add your Forge Client ID, Secret and callback URL. Also define the `ASPNETCORE_URLS` variable. The end result should be as shown below:

```json
"env": {
    "ASPNETCORE_ENVIRONMENT": "Development",
    "ASPNETCORE_URLS" : "http://localhost:3000",
    "FORGE_CLIENT_ID": "your id here",
    "FORGE_CLIENT_SECRET": "your secret here",
    "FORGE_CALLBACK_URL": "http://localhost:3000/api/forge/callback/oauth",
},
```

Run the app. Open `http://localhost:3000` to view your files. It may be required to **Enable my BIM 360 Account** (see app top-right). Click on 

## Deployment

To deploy this application to Heroku, the **Callback URL** for Forge must use your `.herokuapp.com` address. After clicking on the button below, at the Heroku Create New App page, set your Client ID, Secret and Callback URL for Forge.

[![Deploy](https://www.herokucdn.com/deploy/button.svg)](https://heroku.com/deploy)

Watch [this video](https://www.youtube.com/watch?v=Oqa9O20Gj0c) on how deploy samples to Heroku.

# Further Reading

Documentation:

- [BIM 360 API](https://developer.autodesk.com/en/docs/bim360/v1/overview/) and [App Provisioning](https://forge.autodesk.com/blog/bim-360-docs-provisioning-forge-apps)
- [Data Management API](https://developer.autodesk.com/en/docs/data/v2/overview/)
- [Viewer](https://developer.autodesk.com/en/docs/viewer/v6)

Tutorials:

- [View BIM 360 Models](http://learnforge.autodesk.io/#/tutorials/viewhubmodels)
- [Retrieve Issues](https://developer.autodesk.com/en/docs/bim360/v1/tutorials/retrieve-issues)
- [Create Issues](https://forge.autodesk.com/en/docs/bim360/v1/tutorials/issues/create-issues/)

Blogs:

- [Forge Blog](https://forge.autodesk.com/categories/bim-360-api)
- [Field of View](https://fieldofviewblog.wordpress.com/), a BIM focused blog

Other samples:

- [Webhooks notifications](https://github.com/Autodesk-Forge/data.management-nodejs-webhook) (see API documentation [here](https://developer.autodesk.com/en/docs/webhooks/v1/overview/))

### Tips & Tricks

This sample uses .NET Core and works fine on both Windows and MacOS, see [this tutorial for MacOS](https://github.com/augustogoncalves/dotnetcoreheroku).

### Troubleshooting

1. **Cannot see my BIM 360 projects**: Make sure to provision the Forge App Client ID within the BIM 360 Account, [learn more here](https://forge.autodesk.com/blog/bim-360-docs-provisioning-forge-apps). This requires the Account Admin permission.

2. **error setting certificate verify locations** error: may happen on Windows, use the following: `git config --global http.sslverify "false"`

## License

This sample is licensed under the terms of the [MIT License](http://opensource.org/licenses/MIT). Please see the [LICENSE](LICENSE) file for full details.

## Written by

Augusto Goncalves [@augustomaia](https://twitter.com/augustomaia), [Forge Partner Development](http://forge.autodesk.com)
