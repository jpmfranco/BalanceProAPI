# BalancePro API
API RESTful desarrollada en ASP.NET Core 10 para la gestión de finanzas personales. Permite administrar usuarios, ingresos y gastos mediante endpoints documentados con Swagger/OpenAPI.

##  Tecnologías

ASP.NET Core 10
Entity Framework Core
SQL Server 2025
Docker
Swagger / OpenAPI


##  Requisitos previos

.NET 10 SDK
SQL Server o Docker
Visual Studio 2022 o VS Code


## Configuración
1. Clona el repositorio:
bashgit clone https://github.com/jpmfranco/BalancePro.git
cd BalancePro
2. Configura la cadena de conexión en appsettings.json:
json"ConnectionStrings": {
  "DefaultConnection": "Server=TU_SERVIDOR;Database=BalancePro;Trusted_Connection=True;TrustServerCertificate=True"
}
3. Aplica las migraciones:
bashdotnet ef database update
4. Corre el proyecto:
bashdotnet run
5. Accede al Swagger:
http://localhost:7252/swagger

## Correr con Docker
1. Construye la imagen:
bashdocker build -t balancepro-api .
2. O usa Docker Compose desde la raíz del proyecto:
bashdocker-compose up --build

## Endpoints
Usuarios
MétodoEndpointDescripciónGET/api/Usuarios/ObtenerUsuariosObtener todos los usuariosGET/api/Usuarios/ObtenerUsuarioPorID/{id}Obtener usuario por IDPOST/api/Usuarios/CrearUsuarioCrear nuevo usuarioPUT/api/Usuarios/EditarUsuario/{id}Editar usuarioPUT/api/Usuarios/CambiarContrasena/{id}Cambiar contraseña
Gastos
MétodoEndpointDescripciónGET/api/Gastoes/ObtenerGastoObtener todos los gastosGET/api/Gastoes/ObtenerGastoPorID/{id}Obtener gasto por IDGET/api/Gastoes/ObtenerSumaTotalSuma total de gastos por usuarioPOST/api/Gastoes/CrearGastoRegistrar nuevo gastoPUT/api/Gastoes/EditarGasto/{id}Editar gastoDELETE/api/Gastoes/EliminarGasto/{id}Eliminar gasto
Ingresos
MétodoEndpointDescripciónGET/api/Ingresoes/ObtenerIngresoObtener todos los ingresosGET/api/Ingresoes/ObtenerIngresoPorID/{id}Obtener ingreso por IDGET/api/Ingresoes/ObtenerSumaTotalSuma total de ingresos por usuarioPOST/api/Ingresoes/CrearIngresoRegistrar nuevo ingresoPUT/api/Ingresoes/EditarIngreso/{id}Editar ingresoDELETE/api/Ingresoes/EliminarIngreso/{id}Eliminar ingreso

## Modelo de base de datos
<img width="874" height="749" alt="image" src="https://github.com/user-attachments/assets/2f120e3c-fa3a-4928-8922-312f089be3a5" />


## Variables de entorno (Docker)
VariableDescripciónConnectionStrings__DefaultConnectionCadena de conexión a SQL ServerASPNETCORE_URLSPuerto donde corre la APIASPNETCORE_ENVIRONMENTEntorno de ejecución (Development/Production)

## Autores

### Juan Pablo Mayagoitia Franco
### Luis Angel Reyes Valdivia

### Asesor: Jose Juan Meza Espinosa
### CUCEI, Universidad de Guadalajara

## Licencia
Propietario del código — Todos los derechos reservados © 2026
