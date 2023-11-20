# Fishnet Sample Project

This is a simple project to test and learn FishNet capabilities. The project aims to be a simple way to explore common paths of making a multiplayer game with it.

I tried to prioritize game feel and made some decisions to achieve that:
- Player movement is replicated with prediction, to ensure even with latency, the game looks fine from every client. I based the movement on the sample Rigidbody player controller provided with the package, with some tweaks.
- Melee hits are detected and sent from the client of the player making the hit. The only thing that the server validates is distance, so players can't hit another player if they are too far away (because of lag or some other reason). The server also implements a "cooldown" for hits, so no matter how fast hits are sent, you can only damage a player every certain amount of time, giving the damaged a period of invulnerability.
- In the same line of thought, the deaths are also fired from the originating client, but the server validates that said client is actually dead (HP <= 0), before despawning a player.

If I had to add more features, probably would obey this same principle of showing in the player's client their own interactions and detection of events immediately, and then using the server for a simpler validation over what that player is informing and executing pertinent actions.

## Architecture

For simplicity's sake, I used a component based approach, tightly coupled with FishNet's network classes. Given the project is small, I found this approach appropriate, but in a larger scale project, I'll be interested in a more lose coupling with the infrastructure code.

## Technologies used

FishNet 3.11.7R, Unity 2022.3.2

## Challenges

It was a challenge itself learning how FishNet works, and I have to say it's a pleasure to work with. Feels like what Unity's built-in solution should be.

I had some trouble understanding the attributes offered by the framework, but with the documentation and being able to look at the source code, helped a lot to know the ropes.

While FishNet works very well out of the box, some examples of simpler replication/prediction were not working very well. I have yet to know why was that (looked like an issue with floating point precision, maybe), so I opted to base the player movement control on the Rigidbody player control demo.

FishNet animator sync is a Pro edition feature, so I had to find a way to play an animation on the owner client as soon as the player fired it, and then send it to the server so it could replicate the animation to the other clients, sans the original caller. I'm not very happy with the actual implementation, but I assume that with a more clean architecture, this piece of infrastructure code can be better implemented.

The subprocess updating the Scriptable Object containing all network spawnable prefabs keept mangling my references in prefabs, and I still don't know why or how to stop it from happening. 