ARG ASPNETCORE_ENVIRONMENT=production
FROM caddy:2-alpine
ENV ASPNETCORE_ENVIRONMENT=${ASPNETCORE_ENVIRONMENT}
COPY Caddyfile.production /etc/caddy/Caddyfile
COPY www /var/www
