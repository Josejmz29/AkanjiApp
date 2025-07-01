# Usa una imagen oficial de Node como base
FROM node:20-alpine

# Establece el directorio de trabajo dentro del contenedor
WORKDIR /app

# Copia los archivos de configuración y dependencias primero
COPY package.json package-lock.json* ./

# Instala dependencias
RUN npm install

# Copia el resto del código de tu proyecto
COPY . .

# Expone el puerto que Vite usa por defecto
EXPOSE 3000

# Comando por defecto: inicia el servidor de desarrollo de Vite
CMD ["npm", "run", "dev", "--", "--host"]
