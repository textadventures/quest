This directory contains the source code for the Quest Viva website at [questviva.com](https://questviva.com).

To build and run locally:

```
npm install
npm run dev
```
The site has two channels: questviva.com is built from the stable release branch (`release/6.0`), and beta.questviva.com from `main`. To build the beta site, with its banner and `noindex`:

```
PUBLIC_BETA_SITE=true npm run build
```

See "Docs site" in [docs/release-channels.md](../docs/release-channels.md).
