# Proyecto-Inmobiliaria

Sistema web para la gestión de alquileres temporarios de una agencia inmobiliaria.

---

## 👥 Integrantes del Grupo

* **Franco Javier Coria** - *[francojaviercoria2@gmail.com](mailto:francojaviercoria2@gmail.com)* - [@FrancoJCoria](https://github.com/FrancoJCoria)
* **Emiliano Leguizamón** - *[emilegui76@gmail.com](mailto:emilegui76@gmail.com)* - [@Emi-legui](https://github.com/Emi-legui)
* **Matías Agustín Ejarque** - *[ejarque016@gmail.com](mailto:ejarque016@gmail.com)* - [@matejarque](https://github.com/matejarqque)
* **Nahuel Mercado Baravalle Agustin** - *[namercado28@gmail.com](mailto:namercado28@gmail.com)* - [@NahuelMBA](https://github.com/NahuelMBA)

---

## 📐 Modelado de Datos

![Diagrama Entidad-Relación](./docs/diagrama-der.png)

---

## Cómo usar

### Requisitos

- [.NET SDK 10.0](https://dotnet.microsoft.com/download)
- [MySQL Server](https://dev.mysql.com/downloads/mysql/)
- [Postman](https://www.postman.com/downloads/)

### Base de datos

Ejecutá el script `data/script.sql` en MySQL para crear la base de datos y las tablas.

### Configuración

Modificá `appsettings.json` con los datos de tu MySQL:

```json
"ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=inmobiliaria;User ID=root;Password=admin;SslMode=none"
}
```

### Ejecutar

```bash
dotnet run
```

---

## Endpoints (Postman)

En Postman: **Body > raw > JSON** y pegá el JSON de cada ejemplo.

### Propietario

| Operación | Método | URL |
|-----------|--------|-----|
| Alta | `POST` | `/Propietario/Create` |
| Baja | `POST` | `/Propietario/Delete` |
| Modificar | `PUT` | `/Propietario/Edit/{id}` |
| Listar | `GET` | `/Propietario/Index` |

**Alta:**
```json
{
    "dni": "40801072",
    "nombre": "Franco",
    "apellido": "Lopez",
    "telefono": "266378493",
    "email": "francoelopez@gmail.com",
    "estado": true
}
```

**Baja** (se busca por DNI):
```json
{
    "dni": "40801072"
}
```

**Modificar** (reemplazá `{id}` con el ID real):
```json
{
    "dni": "40801072",
    "nombre": "Franco Enrique",
    "apellido": "Lopez",
    "telefono": "266378493",
    "email": "francoelopez7@gmail.com",
    "estado": true
}
```

### Inquilino

| Operación | Método | URL |
|-----------|--------|-----|
| Alta | `POST` | `/Inquilino/Create` |
| Baja | `PATCH` | `/Inquilino/Delete` |
| Modificar | `PUT` | `/Inquilino/Edit/{id}` |
| Listar | `GET` | `/Inquilino/Index` |

**Alta:**
```json
{
    "dni": "45801034",
    "nombre": "Ana",
    "apellido": "Lopez",
    "telefono": "2657246510",
    "email": "mariaelenalopez@gmail.com",
    "estado": true
}
```

**Baja** (se busca por DNI):
```json
{
    "dni": "45801034"
}
```

**Modificar** (reemplazá `{id}` con el ID real):
```json
{
    "dni": "45801034",
    "nombre": "Ana Maria",
    "apellido": "Lopez",
    "telefono": "2657246510",
    "email": "mariaelenalopez@gmail.com",
    "estado": true
}
```
