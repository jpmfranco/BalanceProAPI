# =========================
# MODULO DE EGRESOS
# =========================

# Lista global de gastos
gastos = []

# =========================
# CONVERTIR A MENSUAL
# =========================


def calcular_mensual(monto, frecuencia):

    if frecuencia == "diario":
        return monto * 30
    elif frecuencia == "2_semana":
        return monto * 8
    elif frecuencia == "semanal":
        return monto * 4
    elif frecuencia == "mensual":
        return monto
    else:
        return 0


# =========================
# AGREGAR GASTO
# =========================
def agregar_gasto():
    global gastos

    print("\n--- Nuevo gasto ---")

    categoria = input("Categoria: ")
    nombre = input("Nombre: ")
    monto = float(input("Monto: "))

    print("Frecuencia:")
    print("1. Diario")
    print("2. Dos veces por semana")
    print("3. Semanal")
    print("4. Mensual")

    op = input("Selecciona: ")

    if op == "1":
        frecuencia = "diario"
    elif op == "2":
        frecuencia = "2_semana"
    elif op == "3":
        frecuencia = "semanal"
    elif op == "4":
        frecuencia = "mensual"
    else:
        frecuencia = "mensual"

    print("Clasificacion:")
    print("1. Bueno")
    print("2. Regular")
    print("3. Malo")

    op_c = input("Selecciona: ")

    if op_c == "1":
        clasificacion = "bueno"
    elif op_c == "2":
        clasificacion = "regular"
    elif op_c == "3":
        clasificacion = "malo"
    else:
        clasificacion = "regular"

    gasto = {
        "categoria": categoria,
        "nombre": nombre,
        "monto": monto,
        "frecuencia": frecuencia,
        "clasificacion": clasificacion
    }

    gastos.append(gasto)

    print("Gasto agregado correctamente")


# =========================
# OBTENER DATOS PARA IA
# =========================
def obtener_datos_financieros():
    global gastos

    total = sum(calcular_mensual(g["monto"], g["frecuencia"]) for g in gastos)

    total_malo = sum(
        calcular_mensual(g["monto"], g["frecuencia"])
        for g in gastos if g["clasificacion"] == "malo"
    )

    categorias = {}

    for g in gastos:
        mensual = calcular_mensual(g["monto"], g["frecuencia"])
        categorias[g["categoria"]] = categorias.get(
            g["categoria"], 0) + mensual

    categoria_principal = max(
        categorias, key=categorias.get) if categorias else 0

    return {
        "total_gastos": total,
        "gasto_malo": total_malo,
        "categoria_principal": categoria_principal
    }
