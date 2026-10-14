-- ============================================================================
-- SCRIPT DE BASE DE DATOS: ARENA ELEMENTAL (SQL SERVER & MYSQL)
-- Convencion de Nombres: PascalCase
-- Regla de Balance: VidaBase + DefensaBase + AtaqueBase = 100
-- ============================================================================

-- Creacion de Tablas Normalizadas (PK y FK)
CREATE TABLE Jugadores (
    IdJugador INT IDENTITY(1,1) PRIMARY KEY,
    NombreUsuario VARCHAR(50) NOT NULL UNIQUE,
    ClaveAcceso VARCHAR(100) NOT NULL,
    PartidasJugadas INT DEFAULT 0,
    Victorias INT DEFAULT 0,
    Derrotas INT DEFAULT 0,
    PorcentajeExito DECIMAL(5,2) DEFAULT 0.00
);

CREATE TABLE Personajes (
    IdPersonaje INT PRIMARY KEY,
    Nombre VARCHAR(60) NOT NULL,
    Faccion VARCHAR(20) NOT NULL,
    VidaBase INT NOT NULL,
    DefensaBase INT NOT NULL,
    AtaqueBase INT NOT NULL,
    Empatia INT NOT NULL,
    NombreEspecial VARCHAR(60) NOT NULL,
    DescripcionEspecial VARCHAR(255) NOT NULL,
    CONSTRAINT ChkBalanceCien CHECK ((VidaBase + DefensaBase + AtaqueBase) = 100)
);

CREATE TABLE CartasEscenario (
    IdEscenario INT PRIMARY KEY,
    Nombre VARCHAR(60) NOT NULL,
    FaccionDominante VARCHAR(20) NOT NULL,
    FaccionPenalizada VARCHAR(20) NOT NULL,
    BonoAtaque INT NOT NULL,
    PenalizacionAtaque INT NOT NULL,
    UmbralEmpatia INT NOT NULL,
    Descripcion VARCHAR(255)
);

CREATE TABLE Equipos (
    IdEquipo INT IDENTITY(1,1) PRIMARY KEY,
    IdJugador INT NOT NULL,
    NombreEquipo VARCHAR(50) NOT NULL,
    FOREIGN KEY (IdJugador) REFERENCES Jugadores(IdJugador)
);

CREATE TABLE EquipoDetalle (
    IdDetalle INT IDENTITY(1,1) PRIMARY KEY,
    IdEquipo INT NOT NULL,
    IdPersonaje INT NOT NULL,
    OrdenPosicion INT NOT NULL CHECK (OrdenPosicion BETWEEN 1 AND 5),
    FOREIGN KEY (IdEquipo) REFERENCES Equipos(IdEquipo),
    FOREIGN KEY (IdPersonaje) REFERENCES Personajes(IdPersonaje)
);

CREATE TABLE EstadisticasPartidas (
    IdPartida INT IDENTITY(1,1) PRIMARY KEY,
    IdJugadorUno INT NOT NULL,
    IdJugadorDos INT NOT NULL,
    IdGanador INT NOT NULL,
    IdEscenario INT NOT NULL,
    TotalTurnos INT NOT NULL,
    ArchivoLog VARCHAR(150) NOT NULL,
    FechaCombate DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (IdJugadorUno) REFERENCES Jugadores(IdJugador),
    FOREIGN KEY (IdJugadorDos) REFERENCES Jugadores(IdJugador),
    FOREIGN KEY (IdGanador) REFERENCES Jugadores(IdJugador),
    FOREIGN KEY (IdEscenario) REFERENCES CartasEscenario(IdEscenario)
);

-- ============================================================================
-- INSERCION DEL CATALOGO DE 16 CARTAS (12 PERSONAJES + 4 ESCENARIOS)
-- ============================================================================

INSERT INTO Personajes (IdPersonaje, Nombre, Faccion, VidaBase, DefensaBase, AtaqueBase, Empatia, NombreEspecial, DescripcionEspecial) VALUES
(1, 'Ignis El Piromante', 'Fuego', 45, 20, 35, 40, 'Llamarada Solar', 'Ignora el escudo rival e inflige danio perforante directo.'),
(2, 'Vulcano Acorazado', 'Fuego', 55, 25, 20, 65, 'Erupcion De Magma', 'Funde el escudo enemigo a cero y golpea con magma.'),
(3, 'Kael Cuchilla Ignea', 'Fuego', 40, 20, 40, 30, 'Impacto De Ceniza', 'Ataque critico igneo de alta potencia ofensiva.'),
(4, 'Nereida Del Abismo', 'Agua', 55, 25, 20, 85, 'Marea Sanadora', 'Restaura vida propia, roba energia y ataca.'),
(5, 'Kraken Acorazado', 'Agua', 60, 25, 15, 75, 'Vortice Abisal', 'Drena energia del oponente y regenera salud.'),
(6, 'Siren Cazadora', 'Agua', 50, 20, 30, 60, 'Lanza De Hielo', 'Roba 1 de energia al rival y lanza estocada glacial.'),
(7, 'Golem De Basalto', 'Tierra', 60, 30, 10, 90, 'Muralla Monolitica', 'Refuerza su escudo y descarga un impacto tectonico.'),
(8, 'Terra Guardiana', 'Tierra', 55, 25, 20, 80, 'Raices Aplastantes', 'Suma su defensa al ataque y regenera escudo.'),
(9, 'Rocco Rompemuros', 'Tierra', 50, 25, 25, 70, 'Martillo Tectonico', 'Golpe sismico potenciado por su armadura petrea.'),
(10, 'Cefiro Maestro', 'Aire', 45, 25, 30, 75, 'Tormenta Cortante', 'Ejecuta un impacto doble consecutivo de viento.'),
(11, 'Valkiria Celeste', 'Aire', 50, 20, 30, 80, 'Rafaga Ascendente', 'Doble golpe aereo veloz contra el objetivo.'),
(12, 'Boreas Silencioso', 'Aire', 40, 25, 35, 50, 'Huracan Del Norte', 'Doble rafaga cortante de alta velocidad.');

INSERT INTO CartasEscenario (IdEscenario, Nombre, FaccionDominante, FaccionPenalizada, BonoAtaque, PenalizacionAtaque, UmbralEmpatia, Descripcion) VALUES
(1, 'Volcan Activo', 'Fuego', 'Aire', 6, 4, 70, 'Potencia a Fuego (+6 ATK) y sofoca a Aire (-4 ATK).'),
(2, 'Abismo Oceanico', 'Agua', 'Fuego', 6, 4, 65, 'Potencia a Agua (+6 ATK) y apaga a Fuego (-4 ATK).'),
(3, 'Canion Rocoso', 'Tierra', 'Agua', 6, 4, 75, 'Potencia a Tierra (+6 ATK) y contiene a Agua (-4 ATK).'),
(4, 'Cumbre De Viento', 'Aire', 'Tierra', 6, 4, 70, 'Potencia a Aire (+6 ATK) y erosiona a Tierra (-4 ATK).');

INSERT INTO Jugadores (NombreUsuario, ClaveAcceso, PartidasJugadas, Victorias, Derrotas, PorcentajeExito) VALUES
('JugadorUno', '1234', 0, 0, 0, 0.00),
('JugadorDos', '1234', 0, 0, 0, 0.00);
