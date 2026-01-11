# FoundationCore

This is a .Net Core class library that extends FoundationCommon to provide the Foundation system to .Net Core projects.

Its big pieces of work are:

1.) The definition of the Auditor and Security modules
2.) The base classes for the automatically generated FoundationCore code that is created using application specific database definitions.


This project is to be referenced by applications built on the Foundation platform that are built on .Net Core (.Net 8)


Notes on OIDC certification Generation for use when running OIDC in production mode.

- Use 'Let's Encrypt' to generate a trusted certificate that browsers will respect.

- Generate a Certificate Signing Request (CSR) to get this using OpenSSL.


For example, 

openssl req -new -newkey rsa:2048 -nodes -keyout <URL>.key -out <URL>.csr -subj "/C=<COUNTRY>/ST=<STATE>/L=<LOCALITY>/O=<company>/CN=<URL>"

