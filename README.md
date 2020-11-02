# Walls

A VR dance game prototype

# Licence (MIT)

Copyright 2020 Six to Start Ltd.

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

# Third-party

The prototype uses third-party materials, which for licencing reasons we cannot redistribute with the source code of the project. You will need your own license for these assets in order to run the game in Unity, or you will need to modify the code to not depend on these assets. Required and recommended assets are available for free.

- **Required**: The [SteamVR Plugin](https://assetstore.unity.com/packages/tools/integration/steamvr-plugin-32647) must be installed at `Assets/SteamVR`
- Recommended: The animated water in the "LakeScene" game is generated using [LowPoly Water](https://assetstore.unity.com/packages/tools/particles-effects/lowpoly-water-107563), it should be installed at `Assets/Vendor/LowPolyWater_Pack`.
- Optional: Avenir Next LT Pro font is used for in game text. You can substitute any font.
- Optional: Some decorative assets are used from the [BrokenVector Free Low Poly Pack](https://assetstore.unity.com/packages/3d/free-low-poly-pack-65375). It should be installed at `Assets/Vendor/BrokenVector`.
- Optional: Some decorative assets are used from [Low-Poly Park](https://assetstore.unity.com/packages/3d/environments/urban/low-poly-park-61922). It should be installed at `Assets/Vendor/Park`.
- Optional: [FinalIK](https://assetstore.unity.com/packages/tools/animation/final-ik-14290) is used for pose estimation, which we experimented with in the "PlayScene" version of the game but stopped using when building the final "LakeScene" game. If you choose to explore FinalIK, it must be installed at `Assets/Plugins/RootMotion`. The bundled "Dummy" model should be copied to `Assets/Vendor/Dummy`.

The project, and this repository, includes ["DéplacementRoche.wav" by davidou](https://freesound.org/people/davidou/sounds/88496/), which is licenced and used under the terms of the [Creative Commons 0 licence](https://creativecommons.org/publicdomain/zero/1.0/).

All other included audio is original sound created using Apple Garageband for the prototype.

# Getting Started

- Run SteamVR
  - Make sure everything is up-to-date and green here
- Install Unity 2017.3.1f1
  - When installing, include Microsoft Visual Studio 2017 Community Edition
- Clone this repository
- Open the project in Unity
- Open LakeScene if it isn't already open
- Press play
