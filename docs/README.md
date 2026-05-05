# Quest 5 Docs

Documentation for Quest 5, built with [Jekyll](https://jekyllrb.com/) and the [Just the Docs](https://just-the-docs.com/) theme.

## Serving locally

Install dependencies once:

```
BUNDLE_GEMFILE=Gemfile.dev bundle install
```

Then serve:

```
./serve.sh
```

The first build takes ~35 seconds (757 pages). Incremental rebuilds after editing a file take ~3 seconds.

`Gemfile.dev` is used for local serving to avoid downloading the remote theme on every rebuild. `Gemfile` (with `github-pages`) is used by GitHub Pages for deployment.
