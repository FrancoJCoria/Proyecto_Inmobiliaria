CREATE DATABASE IF NOT EXISTS inmobiliaria;
USE inmobiliaria;

CREATE TABLE Propietario (
  id_propietario int PRIMARY KEY AUTO_INCREMENT,
  dni varchar(255) NOT NULL UNIQUE,
  nombre varchar(255),
  apellido varchar(255),
  telefono varchar(255),
  email varchar(255),
  estado boolean
);

CREATE TABLE Inmueble (
  id_inmueble int PRIMARY KEY AUTO_INCREMENT,
  direccion varchar(255),
  cupo int,
  precio_dia decimal,
  porcentaje_reserva decimal,
  disponible boolean,
  portada varchar(255),
  id_propietario int,
  id_tipo int,
  estado boolean
);

CREATE TABLE TipoInmueble (
  id_tipo int PRIMARY KEY AUTO_INCREMENT,
  nombre varchar(255)
);

CREATE TABLE ImagenInmueble (
  id_imagen int PRIMARY KEY AUTO_INCREMENT,
  url_imagen varchar(255),
  id_inmueble int,
  estado boolean
);

CREATE TABLE Pago (
  id_pago int PRIMARY KEY AUTO_INCREMENT,
  concepto varchar(255),
  fecha_pago date,
  importe decimal,
  estado boolean,
  id_reserva int,
  id_usuario_creador int,
  id_usuario_anulador int
);

CREATE TABLE Reserva (
  id_reserva int PRIMARY KEY AUTO_INCREMENT,
  fecha_inicio date,
  fecha_fin date,
  fecha_fin_efectiva date,
  monto_diario decimal,
  estado boolean,
  id_inmueble int,
  id_inquilino int,
  id_usuario_creador int,
  id_usuario_finalizador int
);

CREATE TABLE Usuario (
  id_usuario int PRIMARY KEY AUTO_INCREMENT,
  email varchar(255),
  clave varchar(255),
  nombre varchar(255),
  apellido varchar(255),
  avatar varchar(255),
  rol varchar(255),
  estado boolean
);

CREATE TABLE Inquilino (
  id_inquilino int PRIMARY KEY AUTO_INCREMENT,
  dni varchar(255),
  nombre varchar(255),
  apellido varchar(255),
  telefono varchar(255),
  email varchar(255),
  estado boolean
);

ALTER TABLE Inmueble ADD FOREIGN KEY (id_propietario) REFERENCES Propietario (id_propietario);

ALTER TABLE Inmueble ADD FOREIGN KEY (id_tipo) REFERENCES TipoInmueble (id_tipo);

ALTER TABLE ImagenInmueble ADD FOREIGN KEY (id_inmueble) REFERENCES Inmueble (id_inmueble);

ALTER TABLE Pago ADD FOREIGN KEY (id_reserva) REFERENCES Reserva (id_reserva);

ALTER TABLE Pago ADD FOREIGN KEY (id_usuario_creador) REFERENCES Usuario (id_usuario);

ALTER TABLE Pago ADD FOREIGN KEY (id_usuario_anulador) REFERENCES Usuario (id_usuario);

ALTER TABLE Reserva ADD FOREIGN KEY (id_inmueble) REFERENCES Inmueble (id_inmueble);

ALTER TABLE Reserva ADD FOREIGN KEY (id_inquilino) REFERENCES Inquilino (id_inquilino);

ALTER TABLE Reserva ADD FOREIGN KEY (id_usuario_creador) REFERENCES Usuario (id_usuario);

ALTER TABLE Reserva ADD FOREIGN KEY (id_usuario_finalizador) REFERENCES Usuario (id_usuario);

-- ============================================================================
-- DATOS DE EJEMPLO (SEED)
-- ============================================================================

-- Tipos de inmueble
INSERT INTO TipoInmueble (id_tipo, nombre) VALUES
  (1, 'Departamento'),
  (2, 'Casa'),
  (3, 'Local');

-- Propietarios
INSERT INTO Propietario (id_propietario, dni, nombre, apellido, telefono, email, estado) VALUES
  (1, '40801072', 'Franco', 'Lopez', '266378493', 'francoelopez@gmail.com', 1),
  (2, '28765432', 'María', 'González', '2657112233', 'mariagonzalez@gmail.com', 1);

-- Usuarios (la clave en texto plano se rehashea a BCrypt en el primer login)
INSERT INTO Usuario (id_usuario, email, clave, nombre, apellido, avatar, rol, estado) VALUES
  (1, 'admin@inmobiliaria.com', '123456', 'Admin', 'Sistema', NULL, 'Administrador', 1),
  (2, 'empleado@inmobiliaria.com', '123456', 'Empleado', 'Demo', NULL, 'Empleado', 1);

-- Inquilinos
INSERT INTO Inquilino (id_inquilino, dni, nombre, apellido, telefono, email, estado) VALUES
  (1, '45801034', 'Ana', 'Lopez', '2657246510', 'mariaelenalopez@gmail.com', 1),
  (2, '36543210', 'Carlos', 'Pereira', '2664455667', 'carlospereira@gmail.com', 1),
  (3, '38987654', 'Lucía', 'Fernández', '2655334455', 'luciafernandez@gmail.com', 1);

-- Inmuebles
INSERT INTO Inmueble (id_inmueble, direccion, cupo, precio_dia, porcentaje_reserva, disponible, portada, id_propietario, id_tipo, estado) VALUES
  (1, 'Av. Illia 123', 4, 15000, 10, 1, '/Uploads/Inmuebles/portada_1.png', 1, 1, 1),
  (2, 'Belgrano 456', 6, 20000, 15, 1, '/Uploads/Inmuebles/portada_2.png', 1, 2, 1),
  (3, 'Pueyrredón 789', 2, 9000, 5, 1, '/Uploads/Inmuebles/portada_3.png', 2, 3, 1),
  (4, 'San Martín 321', 5, 18000, 10, 0, '', 2, 1, 1),
  (5, 'Rivadavia 654', 8, 25000, 20, 1, '', 1, 2, 1),
  (6, 'Mitre 987', 3, 12000, 10, 1, '', 2, 1, 1);

-- Imágenes de inmuebles
INSERT INTO ImagenInmueble (id_imagen, url_imagen, id_inmueble, estado) VALUES
  (1, '/Uploads/Inmuebles/1/54598973-45cf-4ca1-988e-e132d63a1b13.png', 1, 1),
  (2, '/Uploads/Inmuebles/1/5e7f509e-df21-43bc-8bb9-36da38134eb7.png', 1, 1),
  (3, '/Uploads/Inmuebles/2/14037950-efc1-4d75-9ff3-1ed3571d391f.png', 2, 1),
  (4, '/Uploads/Inmuebles/2/2e7064e6-6cbf-45bd-8687-c6df77880532.png', 2, 1),
  (5, '/Uploads/Inmuebles/3/91063bb7-49e0-4242-bdd9-aee39b912a3b.png', 3, 1);

-- Reservas
INSERT INTO Reserva (id_reserva, fecha_inicio, fecha_fin, fecha_fin_efectiva, monto_diario, estado, id_inmueble, id_inquilino, id_usuario_creador, id_usuario_finalizador) VALUES
  (1, CURDATE(), DATE_ADD(CURDATE(), INTERVAL 5 DAY), DATE_ADD(CURDATE(), INTERVAL 5 DAY), 15000, 1, 1, 1, 1, 2),
  (2, DATE_ADD(CURDATE(), INTERVAL 7 DAY), DATE_ADD(CURDATE(), INTERVAL 12 DAY), NULL, 20000, 1, 2, 2, 1, NULL),
  (3, DATE_SUB(CURDATE(), INTERVAL 10 DAY), DATE_SUB(CURDATE(), INTERVAL 5 DAY), DATE_SUB(CURDATE(), INTERVAL 5 DAY), 9000, 0, 3, 3, 1, 2);

-- Pagos
INSERT INTO Pago (id_pago, concepto, fecha_pago, importe, estado, id_reserva, id_usuario_creador, id_usuario_anulador) VALUES
  (1, 'Reserva #1 - Señal', CURDATE(), 3000, 1, 1, 1, NULL),
  (2, 'Reserva #2 - Pago completo', DATE_ADD(CURDATE(), INTERVAL 1 DAY), 240000, 1, 2, 1, NULL);