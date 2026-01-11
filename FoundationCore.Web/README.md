# FoundationCore.Web

This is a .Net Core (.Net 9) class library that provides the basic common functionality to be used by all Foundation web applications developed
on .Net Core.

It provides things that are reusable across projects like:

- A base implementation of a controller that allows for controllers that have access to foundation service to be developed
- Base classes for SignalR hubs, as well as the standard Alert Hub to use in all apps.
- An extension for supporting the binding of a Kestrel instance to specific controllers

