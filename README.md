# GalaxyBudsClient_Headtracking

🧠 Why This Project?
When building spatial audio content for XR, it's essential to be able to test real head movements directly inside Unity.

This tool bridges that gap using an affordable, wireless, and mobile-friendly solution — the sensors embedded in Galaxy Buds.

This project is a modified version of the original [GalaxyBudsClient](https://github.com/timschneeb/GalaxyBudsClient), focused on capturing spatial head-tracking data from Samsung Galaxy Buds and streaming it via OSC to Unity (or any compatible receiver).

It enables fast testing and prototyping of spatial audio systems inside a game engine using real head orientation data.

## 🎧 Features

- Real-time quaternion stream from Galaxy Buds (4D rotation vector)
- OSC output (default: `/buds/orientation` on port `9000`)
- Unity-compatible orientation mapping (adjustable)
- Keyboard-driven mapping exploration (test permutations live)
- Calibration system with one key press
- Persistent mapping preference

---

## 🚀 Getting Started

### Requirements

- Samsung Galaxy Buds (Pro or similar with spatial tracking)
- Windows 10+
- .NET 8 SDK
- Unity (for OSC receiver, optional)
- OSC receiver (e.g., [extOSC](https://github.com/Iam1337/extOSC) in Unity)

---

🎛️ Controls
- Key	Action
- C	Calibrate orientation (set current rotation as forward)
- Space	Cycle through axis mapping permutations
- Enter	Confirm current mapping as preferred (saved for next launch)

The selected mapping is persisted and reused automatically on next startup.

---

🛠️ Credits
Based on timschneeb/GalaxyBudsClient
