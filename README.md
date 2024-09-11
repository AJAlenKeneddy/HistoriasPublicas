🚀 Avances en el Proyecto StoryBlaze 🚀
StoryBlaze es una plataforma interactiva para crear, gestionar y votar historias, donde los usuarios pueden colaborar agregando fragmentos y comentando. Los avances incluyen:

Funcionalidades principales de StoryBlaze:
Autenticación con JWT:

Implementación de autenticación y manejo de sesión mediante tokens JWT.
Validación de tokens usando localStorage y verificación a través del archivo auth.js ubicado en el directorio wwwroot/js/.
Redirección basada en la autenticación: los usuarios son redirigidos a "Mis Historias" o la página de inicio de sesión según su token.
Manejo de Historias:

Los usuarios pueden crear, editar y gestionar sus historias.
Se permite la interacción de múltiples usuarios a través de un sistema de turnos.
Listado de historias en un componente con redirección mediante clics en las filas de la tabla a la ruta /historia/{HistoriaId:int}.
Interacción con fragmentos y votos:

Implementación de un sistema de votación por fragmentos.
Los usuarios pueden ver historias completas junto con sus fragmentos, comentarios y votos.
Componentes Personalizados:

Se creó un componente HistoriaTable que lista historias con diseño responsivo.
Se desarrolló el componente CategoriasSection para mostrar categorías en un diseño de tres columnas.
Tecnologías utilizadas:
Blazor Server: Utilizado para la interfaz de usuario interactiva.
.NET Core: Back-end de la aplicación.
JWT (JSON Web Tokens): Para el manejo de autenticación.
EF Core: Interacción con la base de datos.
SQL Server: Motor de base de datos.
JavaScript: Para funciones específicas como el manejo de tokens.
