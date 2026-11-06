# Racha web

Angular 22 frontend for Racha. See the [root README](../README.md) for the full setup (database and APIs).

```bash
npm install
npm start            # http://localhost:4200 (expects the APIs on :5080 and :5090)
npm test             # unit tests (Vitest), watch mode
npm run test:ci      # single run
npm run build        # production build in dist/
npm run format       # Prettier
```

API URLs are configured in `src/environments/environment.ts`.
