echo ##" 🎵YoutubeStats"

API musical desarrollada en **ASP.NET Core** que permite buscar, explorar y reproducir contenido de YouTube, incluyendo soporte para:

- 🔎 Búsqueda de videos (Con key Api o YoutubeExplode si vencen las claves o si directamente no funcionan )
- 📃 Información detallada de playlists
- ▶️ Reproducción de canciones con YouTubeExplode, individuales o listas completas(la reproduccion de playlist se vera en el front mas adelante)
- ⭐ Favoritos y playlists personalizadas
- 📈 Estadísticas e historial de reproducciones
- 💾 Descarga de MP3(Single y Playlists)
- 🚀 Listo para ser consumido desde un frontend moderno (React, Angular, etc.)

---

## ⚙️ Tecnologías usadas

- \`ASP.NET Core 8.0\`
- \`Entity Framework Core\`
- \`YoutubeExplode\`
- \`HttpClient + Google API\`
- \`AutoMapper\`
- \`MemoryCache\`
- \`JWT (en progreso)\`
- \`Swagger\`

---

## 🧠 Funcionalidades principales

### 🎧 Búsqueda

\`\`\`http
GET /api/search?q=nombre_del_artista_o_video
\`\`\`

Retorna una lista de videos de YouTube con:
- ID
- Título
- Canal
- Miniatura
- Duración

🎯 También detecta si es una playlist e invoca el método correspondiente.

---

### 📂 Obtener detalles de una playlist

\`\`\`http
GET /api/search/playlist/{playlistId}
\`\`\`

Devuelve:
- Título de la playlist
- Autor
- Lista de videos con título, duración, canal, etc.

---

### ⏺️ Reproducción

El backend entrega datos estructurados listos para que el frontend permita:
- Reproducir canciones individuales
- Elegir cualquier video de una playlist
- Mostrar información completa del video

---

### 📌 Favoritos

Permite guardar videos favoritos para el usuario (estructura lista para integrar autenticación JWT).

---

### 🧠 Historial

Guarda las canciones reproducidas recientemente, incluyendo tiempo de reproducción mínima, para evitar duplicados.

---

### 📉 Estadísticas

Registra:
- Canciones más escuchadas
- Artistas más reproducidos
- Recuento total de reproducciones

---

### 📥 Descarga en MP3

Funcionalidad opcional con soporte para convertir y entregar archivos \`.mp3\` desde YouTube directamente.
Descarga playlists completas con ID, devuelve un zip y si falla alguna cancion, sigue la descarga y devuelve el .zip con las canciones + txt con la lista de errores.

---

## 🧪 Testing local

\`\`\`bash
dotnet run
\`\`\`

- Swagger disponible en: \`http://localhost:5284/swagger\`

---

## ☁️ Despliegue

Puedes desplegar este proyecto en:
- **Render**
- **Fly.io**
- **Azure App Service**
- **Railway**

---

## 🚧 Próximas mejoras e ideas

- [ ] Autenticación con JWT
- [ ] Agregar New Releases con Api de Spotify
- [ ] Modo aleatorio y repetición
- [ ] Likes y etiquetas por usuario
- [ ] Exportar historial en CSV/JSON

---

## 🤝 Contribuciones

¡Las contribuciones son bienvenidas! Si encuentras un bug o tienes ideas para nuevas funciones, no dudes en abrir un issue.

---

## 📜 Licencia

MIT © [Mauro Cosentino](https://github.com/maurocosentino)
" > README.md
