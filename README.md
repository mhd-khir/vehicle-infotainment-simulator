# Project Name: Infotainment Simulator

A Unity prototype of a vehicle infotainment system, built for the IDEENION technical assessment. 
I created two widgets:
1- The **Home** screen shows current weather and nearby EV charging stations.
2- The **Lighting** screen lets you change the cabin light and preview the Welcome and Goodbye sequences.

## Used Unity Version

- **Unity:** 6.3 (`6000.3.28f1`)
- **Render pipeline:** Universal Render Pipeline (URP)

## Steps Of Usages:
1. Clone the repository and open the project folder in Unity Hub using the editor version above.
2. Open `Assets/Scenes/Infotainment.unity`., here you can find the scene contains the simulator
3. Press **Play**. then by pressing **Home** and **Lighting** buttons, you can switch screens.

## Widegts Properties

- **Weather Widget:** Shows the current temperature and conditions from Open-Meteo API.
- **Charging stations:** Shows up to three nearby stations from Open Charge Map, ordered by distance among the results returned by the API. The widget shows charging power when available. Station information does not indicate real-time availability.
- **Ambient lighting:** Pick a color preset and use the brightness slider to adjust the cabin light. "there are 4 available colors for cabin light"
- **Welcome and Goodbye:** Welcome brings up the exterior lights as the cabin light fades in. Goodbye fades out the cabin light while the rear lights briefly stay on. Changing the color or brightness during a sequence returns control to the user.

## Design decisions

I used a top-down 2D car sprite so the front, rear, and cabin lighting can be seen together.
I kept live information on Home and lighting controls on a separate screen so each screen has a clear purpose.

Both widgets share the same request flow, while each handles its own API data and presentation. I show separate states for loading, an empty response, and a failed request, so a user can tell whether the app is still waiting, found nothing, or needs a retry.

The lighting sequences apply a temporary effect to the cabin light while keeping the user's brightness setting separate. This lets the sequences fade the light without replacing the setting on the slider.

## Tests

I added 25 EditMode unit test cases to check how the app handles missing or invalid weather data, filters and sorts charging stations, and preserves the user's brightness setting during lighting sequences.

To open the tests window:
Window Menu -> General -> **Test Runner**.
in the **Test Runner** window select **EditMode**, and right-click on **Infotainment-Simulator**, that will run All 25 test cases.
