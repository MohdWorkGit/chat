# Stage 1: Build
FROM node:20-alpine AS build
WORKDIR /app

COPY package.json ./
RUN npm install

COPY . .
RUN npx ng build --configuration production

# Stage 2: Run SSR with Node
FROM node:20-alpine AS runtime
WORKDIR /app

RUN addgroup -S appgroup && adduser -S appuser -G appgroup

COPY --from=build /app/dist/portal ./dist/portal
COPY --from=build /app/package.json ./

# Install production dependencies only (express)
RUN npm ci --omit=dev 2>/dev/null || npm install --omit=dev express

USER appuser

EXPOSE 4000
ENV PORT=4000

HEALTHCHECK --interval=30s --timeout=5s --retries=3 \
    CMD wget -qO- http://localhost:4000/ || exit 1

CMD ["node", "dist/portal/server/server.mjs"]
