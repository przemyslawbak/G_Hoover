# G_Hoover

## Purpose

Desktop WPF application created for private and commercial usage. The solution is using popular search engine to get specified data, bypassing captcha verifications.

## Features

1. Possible to load a list of phrases to be searched from file.
2. Controls for start, pause, stop of browsing.
3. Possible to swap connection type between Tor network and direct connection in runtime.
4. User is able to change browsing input between JavaScript input and hardware input simulation.
5. Dialog box for building complex search phrase, where loaded phrase is included.
6. Possible to change browsing IP when browsing.
7. Application is designed to solve audio captcha challenges.
8. Recording audio samples.
9. Using API for audio analysis.

## Technology

1. Approaches:
  - Cefsharp browser wrapper,
  - MVVM pattern,
  - service layer in separate directory,
  - async commands,
  - events (Prism and event handlers),
  - IoC container (Autofac),
  - resolving view models in ViewModelLocator class,
  - async methods,
  - task cancellation for browsing,
  - btn images resource,
  - MVVM for dialogs.
  
2. Application is using:
  - C#, WPF,
  - Autofac,
  - Cefsharp WPF,
  - InputSimulator,
  - MvvmDialogs,
  - Prism.Core,
  - CSCore,
  - Microsoft.CognitiveServices.Speech.

## Production

28 Sep 2019
