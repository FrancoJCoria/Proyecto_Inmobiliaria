CREATE DATABASE IF NOT EXISTS `inmobiliaria`;
USE `inmobiliaria`;

CREATE TABLE `Propietario` (
  `id_propietario` int PRIMARY KEY AUTO_INCREMENT,
  `dni` varchar(255),
  `nombre` varchar(255),
  `apellido` varchar(255),
  `telefono` varchar(255),
  `email` varchar(255),
  `estado` boolean
);

CREATE TABLE `Inmueble` (
  `id_inmueble` int PRIMARY KEY AUTO_INCREMENT,
  `direccion` varchar(255),
  `cupo` int,
  `precio_dia` decimal,
  `porcentaje_reserva` decimal,
  `disponible` boolean,
  `portada` varchar(255),
  `id_propietario` int,
  `id_tipo` int,
  `estado` varchar(255)
);

CREATE TABLE `TipoInmueble` (
  `id_tipo` int PRIMARY KEY AUTO_INCREMENT,
  `nombre` varchar(255)
);

CREATE TABLE `ImagenInmueble` (
  `id_imagen` int PRIMARY KEY AUTO_INCREMENT,
  `url_imagen` varchar(255),
  `id_inmueble` int,
  `estado` varchar(255)
);

CREATE TABLE `Pago` (
  `id_pago` int PRIMARY KEY AUTO_INCREMENT,
  `concepto` varchar(255),
  `fecha_pago` date,
  `importe` decimal,
  `estado` varchar(255),
  `id_reserva` int,
  `id_usuario_creador` int,
  `id_usuario_anulador` int
);

CREATE TABLE `Reserva` (
  `id_reserva` int PRIMARY KEY AUTO_INCREMENT,
  `fecha_inicio` date,
  `fecha_fin` date,
  `fecha_fin_efectiva` date,
  `monto_diario` decimal,
  `estado` boolean,
  `id_inmueble` int,
  `id_inquilino` int,
  `id_usuario_creador` int,
  `id_usuario_finalizador` int
);

CREATE TABLE `Usuario` (
  `id_usuario` int PRIMARY KEY AUTO_INCREMENT,
  `email` varchar(255),
  `clave` varchar(255),
  `nombre` varchar(255),
  `apellido` varchar(255),
  `avatar` varchar(255),
  `rol` varchar(255),
  `estado` boolean
);

CREATE TABLE `Inquilino` (
  `id_inquilino` int PRIMARY KEY AUTO_INCREMENT,
  `dni` varchar(255),
  `nombre` varchar(255),
  `apellido` varchar(255),
  `telefono` varchar(255),
  `email` varchar(255),
  `estado` boolean
);

ALTER TABLE `Inmueble` ADD FOREIGN KEY (`id_propietario`) REFERENCES `Propietario` (`id_propietario`);

ALTER TABLE `Inmueble` ADD FOREIGN KEY (`id_tipo`) REFERENCES `TipoInmueble` (`id_tipo`);

ALTER TABLE `ImagenInmueble` ADD FOREIGN KEY (`id_inmueble`) REFERENCES `Inmueble` (`id_inmueble`);

ALTER TABLE `Pago` ADD FOREIGN KEY (`id_reserva`) REFERENCES `Reserva` (`id_reserva`);

ALTER TABLE `Pago` ADD FOREIGN KEY (`id_usuario_creador`) REFERENCES `Usuario` (`id_usuario`);

ALTER TABLE `Pago` ADD FOREIGN KEY (`id_usuario_anulador`) REFERENCES `Usuario` (`id_usuario`);

ALTER TABLE `Reserva` ADD FOREIGN KEY (`id_inmueble`) REFERENCES `Inmueble` (`id_inmueble`);

ALTER TABLE `Reserva` ADD FOREIGN KEY (`id_inquilino`) REFERENCES `Inquilino` (`id_inquilino`);

ALTER TABLE `Reserva` ADD FOREIGN KEY (`id_usuario_creador`) REFERENCES `Usuario` (`id_usuario`);

ALTER TABLE `Reserva` ADD FOREIGN KEY (`id_usuario_finalizador`) REFERENCES `Usuario` (`id_usuario`);
