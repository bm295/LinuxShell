FROM node:22-alpine AS build
WORKDIR /app

COPY src/frontend/football-manager-ui/package*.json ./
RUN npm ci

COPY src/frontend/football-manager-ui/ ./

RUN npm run build

FROM nginx:1.27-alpine AS runtime
COPY docker/nginx/default.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist/football-manager-ui/browser /usr/share/nginx/html

EXPOSE 80
