# SharpENDEC

### Credits
```
BunnyTub - Main Developer
ApatheticDELL - Logo Designer
Google Translate / Microsoft Translator - Translations
```

### Libraries
```
Costura.Fody
Fody
NAudio
Newtonsoft.Json
```

> [!NOTE]  
> Although SharpENDEC is meant to replicate an ENDEC, it is **not** meant to be a complete ENDEC replacement.
> Physical hardware is usually more stable, and more capable in terms of processing.

## What is it anyway?
SharpENDEC is a Canadian software CAP (Common Alerting Protocol) receiver for alerting.
If you're a little confused, imagine something similar to a physical ENDEC, but based in software.

## How do I install it?
Well, it's simple! You don't need to. Simply put the executable into a directory of your choice, then run it. If you run into a problem, or you're on an older version of Windows, you'll most likely need to [install .NET Framework 4.8.1](https://dotnet.microsoft.com/en-us/download/dotnet-framework/net481), or fully update Windows. Once you've done that, there isn't anything else that you will need to install or change.

## How do I set it up?
Well, you don't have to. But you should! There's a lot of stuff that could be of use. But... you should look over [this PDF document from Pelmorex (2021/06)](https://alerts.pelmorex.com/wp-content/uploads/2021/06/NAADS-LMD-User-Guide-R10.0.pdf) for some more knowledge on how Canada's public alerting system works, and its CAP-CP XML data. Once you have a little bit of a better understanding of the system, even just a little bit, come back here!

Here's something, configurations! You might want to change some default settings, especially if they don't fit for your scenario. If you want to change your settings, press ```C``` during startup. To reset them to factory defaults, press ```R``` instead. I'll let you explore them for yourself! Although... if you can't find any CAP-GP geocodes for use in filtering, you can use [this Excel spreadsheet](https://www.publicsafety.gc.ca/cnt/rsrcs/pblctns/capcp-lctn-rfrncs/capcp-lctn-rfrncs-annex-a-201708.xlsx), which contains the most recent list of geocodes.

## How can I change the audio?
Audio can be added and changed easily! Simply add in files and swap as you please.

```in.wav``` This sound is used as the lead-in, and is played before all other sounds. It will simply be ignored if it does not exist.

```attn.wav``` This sound is used as the attention tone for alerts marked as severe and above. The built-in copy will be extracted if it does not exist.

```attn-minor.wav``` This sound is used as the attention tone for alerts marked as minor and below. It will fallback to **attn.wav** if it does not exist.

```audio.wav``` This sound is reserved for the program's internal use. Modifying the file should not have any effect, and will be overwritten.

```out.wav``` This sound is used as the lead-out, and is played after all other sounds. It will simply be ignored if it does not exist.

## Is there anything else I need to know?
Nope! That's about it. There's nothing else to know here. Thanks ApatheticDELL for this application idea, it worked out really well, and you were also awesome for helping me figure out these things. SharpENDEC (C#) was originally ported from QuantumENDEC (Python), and that was uh, really hard, and annoying to figure out. I did not like translating Python, but once it got going well, everything fit into place, and overall, felt good enough, then later on, I made it better.
