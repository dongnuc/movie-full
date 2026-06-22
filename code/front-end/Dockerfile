# =========================
# BUILD STAGE
# =========================
FROM node:20-alpine AS build

WORKDIR /app

COPY package*.json ./
RUN npm ci

COPY . .
RUN npm run build -- --configuration production

# =========================
# RUNTIME STAGE
# =========================
FROM nginx:alpine

# Copy nginx config if needed, or use default
COPY --from=build /app/dist/client/browser /usr/share/nginx/html

# Expose port 80
EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]
