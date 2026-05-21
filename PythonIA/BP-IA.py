import pandas as pd
from sklearn.model_selection import train_test_split
from sklearn.linear_model import LinearRegression
from flask import Flask, request, jsonify
from flask_cors import CORS

app = Flask(__name__)
CORS(app)  # Esto evita errores de permisos (CORS)

# 1. Cargar y Entrenar el modelo al iniciar
print("Cargando datos y entrenando modelo...")
data = pd.read_csv("data-set.csv")
X = data.drop("expectativa", axis=1)
y = data["expectativa"]
X_train, X_test, y_train, y_test = train_test_split(X, y, test_size=0.2)
modelo = LinearRegression()
modelo.fit(X_train, y_train)
print("Modelo entrenado y listo.")

# 2. Definir la ruta que .NET está buscando


@app.route('/predict', methods=['POST'])
def predict():
    try:
        datos_entrada = request.json
        # Tomamos el ingreso que viene de .NET (el que puso el usuario)
        ingreso_usuario = float(datos_entrada.get("ingresos", 0))

        # Preparamos los datos para el modelo Scikit-Learn
        usuario_df = pd.DataFrame([{
            "perfil": datos_entrada.get("perfil", 1),
            "ingresos": ingreso_usuario,
            "estabilidad": datos_entrada.get("estabilidad", 1),
            "gasto_pct": datos_entrada.get("gasto_pct", 50),
            "categoria": datos_entrada.get("categoria", 1),
            "tend_ingresos": datos_entrada.get("tend_ingresos", 1),
            "tend_egresos": datos_entrada.get("tend_egresos", 0),
            "control": datos_entrada.get("control", 3),
            "planificacion": datos_entrada.get("planificacion", 3)
        }])[X.columns]

        # El modelo predice una "expectativa" (generalmente un valor cercano a 1)
        prediccion = modelo.predict(usuario_df)
        tendencia = float(prediccion[0])

        # --- LÓGICA DE PROYECCIÓN DINÁMICA ---
        # Usamos un factor de suavizado (0.02) para que la IA no invente crecimientos locos.
        # Esto significa que la IA influirá máximo en un 2-5% del sueldo real.
        ajuste_ia = (tendencia * 0.02)

        mes1 = ingreso_usuario * (1 + ajuste_ia)
        # El efecto se suaviza en el tiempo
        mes2 = mes1 * (1 + (ajuste_ia * 0.7))
        mes3 = mes2 * (1 + (ajuste_ia * 0.4))

        print(
            f"Ingreso Base: {ingreso_usuario} -> Proyección: {mes1}, {mes2}, {mes3}")

        return jsonify({
            "status": "success",
            "proyecciones": [
                {"mes": "Próximo Mes", "valor": round(mes1, 2)},
                {"mes": "Mes 2", "valor": round(mes2, 2)},
                {"mes": "Mes 3", "valor": round(mes3, 2)}
            ]
        })

    except Exception as e:
        print(f"Error: {e}")
        return jsonify({"error": str(e)}), 500


if __name__ == '__main__':
    # Ejecutar en el puerto 5000
    app.run(host='0.0.0.0', port=5002)
