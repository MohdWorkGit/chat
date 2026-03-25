# Stage 1: Build
FROM node:20-alpine AS build
WORKDIR /app

COPY package.json ./
RUN npm install

COPY . .
RUN npx ng build --configuration production

# Stage 2: Serve with nginx
FROM nginx:alpine AS runtime

COPY --from=build /app/dist/widget/browser /usr/share/nginx/html
COPY <<'EOF' /etc/nginx/conf.d/default.conf
server {
    listen 80;
    server_name _;
    root /usr/share/nginx/html;

    # Serve widget assets with permissive CORS for embedding
    location / {
        add_header Access-Control-Allow-Origin *;
        add_header Access-Control-Allow-Methods "GET, OPTIONS";
        add_header Access-Control-Allow-Headers "Content-Type";
        try_files $uri $uri/ /index.html;
    }

    location /api/ {
        proxy_pass http://api:8080/api/;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection "upgrade";
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }

    # Cache static assets aggressively
    location ~* \.(js|css|woff2?|ttf|eot|svg|png|jpg|ico)$ {
        add_header Access-Control-Allow-Origin *;
        expires 1y;
        add_header Cache-Control "public, immutable";
    }
}
EOF

HEALTHCHECK --interval=30s --timeout=5s --retries=3 \
    CMD wget -qO- http://localhost:80/ || exit 1

EXPOSE 80
