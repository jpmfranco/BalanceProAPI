# =========================
# IMPORTACIONES
# =========================
import pandas as pd
from sklearn.model_selection import train_test_split
from sklearn.linear_model import LinearRegression

from expenses import agregar_gasto, obtener_datos_financieros

# =========================
# MODELO
# =========================
data = pd.read_csv("data-set.csv")

X = data.drop("expectativa", axis=1)
y = data["expectativa"]

X_train, X_test, y_train, y_test = train_test_split(X, y, test_size=0.2)

modelo = LinearRegression()
modelo.fit(X_train, y_train)

# =========================
# VARIABLES GLOBALES
# =========================
perfil = None
ingresos = 0
estabilidad = 0
categoria = 0
tend_ingresos = 0
tend_egresos = 0
control = 0
planificacion = 0

# =========================
# FUNCION PERFIL
# =========================


def crear_perfil():
    global perfil, ingresos, estabilidad, categoria
    global tend_ingresos, tend_egresos, control, planificacion

    print("\n--- CREAR PERFIL ---\n")

    perfil = int(
        input("Perfil (0 estudiante, 1 empleado, 2 independiente, 3 empresario): "))
    ingresos = float(input("Ingresa tu ingreso mensual: "))
    estabilidad = int(input("Ingresos estables? (1 si / 0 no): "))
    categoria = int(input(
        "Categoria principal de gasto (0 vivienda,1 transporte,2 comida,3 entretenimiento): "))
    tend_ingresos = int(
        input("Tendencia ingresos (1 sube, 0 igual, -1 baja): "))
    tend_egresos = int(input("Tendencia egresos (1 sube, 0 igual, -1 baja): "))
    control = int(input("Control financiero (1-5): "))
    planificacion = int(input("Planificacion financiera (1-5): "))

    print("\nPerfil creado correctamente")


# =========================
# FUNCION ANALISIS
# =========================
def analizar():
    if perfil is None:
        print("Primero debes crear un perfil")
        return

    datos = obtener_datos_financieros()

    total_gastos = datos["total_gastos"]
    gasto_malo = datos["gasto_malo"]

    gasto_pct = (total_gastos / ingresos) * 100 if ingresos > 0 else 0
    gasto_malo_pct = (gasto_malo / total_gastos) * \
        100 if total_gastos > 0 else 0

    usuario = pd.DataFrame([{
        "perfil": perfil,
        "ingresos": ingresos,
        "estabilidad": estabilidad,
        "gasto_pct": gasto_pct,
        "categoria": categoria,
        "tend_ingresos": tend_ingresos,
        "tend_egresos": tend_egresos,
        "control": control,
        "planificacion": planificacion
    }])

    pred = modelo.predict(usuario)[0]

    # Ajuste inteligente
    ajuste = 0

    if gasto_pct > 70:
        ajuste -= 0.3
    elif gasto_pct > 50:
        ajuste -= 0.15

    if gasto_malo_pct > 40:
        ajuste -= 0.25
    elif gasto_malo_pct > 20:
        ajuste -= 0.1

    pred_final = pred + ajuste

    # Proyección
    mes1 = ingresos * (1 + (pred_final * 0.05))
    mes2 = mes1 * (1 + (pred_final * 0.03))
    mes3 = mes2 * (1 + (pred_final * 0.02))

    print("\n--- PROYECCION ---")
    print("Mes 1:", round(mes1, 2))
    print("Mes 2:", round(mes2, 2))
    print("Mes 3:", round(mes3, 2))

    # Análisis
    print("\n--- ANALISIS ---")

    if gasto_pct > 70:
        print("Estas gastando demasiado")
    elif gasto_pct > 50:
        print("Gasto moderadamente alto")
    else:
        print("Buen control de gastos")

    if gasto_malo_pct > 30:
        print("Muchos gastos innecesarios detectados")

    print("Categoria principal:", datos["categoria_principal"])

    # Simulación
    print("\n--- SIMULACION ---")

    ahorro = gasto_malo * 0.5
    nuevo_balance = ingresos - (total_gastos - ahorro)

    print("Podrias ahorrar:", round(ahorro, 2))
    print("Nuevo balance:", round(nuevo_balance, 2))


# =========================
# MENU PRINCIPAL
# =========================
while True:

    print("\n=== BALANCE PRO ===")
    print("1. Crear perfil")
    print("2. Agregar egreso")
    print("3. Ver analisis")
    print("4. Salir")

    op = input("Selecciona: ")

    if op == "1":
        crear_perfil()

    elif op == "2":
        agregar_gasto()

    elif op == "3":
        analizar()

    elif op == "4":
        print("Saliendo...")
        break

    else:
        print("Opcion invalida")
