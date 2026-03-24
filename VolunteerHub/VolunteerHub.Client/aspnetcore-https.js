// ======================================
// Author: K2 Research
// Copyright (c) 2026 www.k2research.ca
// 
// 
// ======================================

// This script sets up HTTPS for the application using the ASP.NET Core HTTPS certificate.
// It always regenerates the exported PEM files from the current dev cert to prevent
// stale/expired certificate errors during development.
const fs = require('fs');
const spawn = require('child_process').spawn;
const path = require('path');

const baseFolder =
  process.env.APPDATA !== undefined && process.env.APPDATA !== ''
    ? `${process.env.APPDATA}/ASP.NET/https`
    : `${process.env.HOME}/.aspnet/https`;

const certificateArg = process.argv.map(arg => arg.match(/--name=(?<value>.+)/i)).filter(Boolean)[0];
const certificateName = certificateArg ? certificateArg.groups.value : process.env.npm_package_name;

if (!certificateName) {
  console.error('Invalid certificate name. Run this script in the context of an npm/yarn script or pass --name=<<app>> explicitly.')
  process.exit(-1);
}

const certFilePath = path.join(baseFolder, `${certificateName}.pem`);
const keyFilePath = path.join(baseFolder, `${certificateName}.key`);

//
// Always remove old exported certs and re-export from the current dev certificate.
// This ensures the Angular dev server always uses a valid, non-expired cert.
//
try { fs.unlinkSync(certFilePath); } catch (e) { /* ignore if not found */ }
try { fs.unlinkSync(keyFilePath); } catch (e) { /* ignore if not found */ }

spawn('dotnet', [
  'dev-certs',
  'https',
  '--export-path',
  certFilePath,
  '--format',
  'Pem',
  '--no-password',
], { stdio: 'inherit' })
.on('exit', (code) => process.exit(code));
