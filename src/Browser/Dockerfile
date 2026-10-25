FROM node:24-alpine AS build
WORKDIR /source

COPY package.json package-lock.json ./
RUN npm ci

COPY . .
RUN npm run build

FROM nginxinc/nginx-unprivileged:alpine
COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY docker-entrypoint.sh /docker-entrypoint.d/40-mayfly-config.sh
USER root
RUN chmod 755 /docker-entrypoint.d/40-mayfly-config.sh
USER nginx
COPY --from=build --chown=nginx:nginx /source/dist /usr/share/nginx/html

EXPOSE 8080
