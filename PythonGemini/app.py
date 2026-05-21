import google.generativeai as genai
from flask import Flask, request, jsonify
from flask_cors import CORS

app = Flask(__name__)
CORS(app)

# Configura tu llave aquí
genai.configure(api_key="AIzaSyADCht7fU-XovPZtmykLHI0FVbPDbVpvkM")
model = genai.GenerativeModel(model_name='gemini-2.5-flash')

print("Modelos disponibles para tu cuenta:")
for m in genai.list_models():
    if 'generateContent' in m.supported_generation_methods:
        print(m.name)


@app.route('/analizar-ahorro', methods=['POST'])
def analizar_ahorro():
    data = request.json
    gastos_malos = data.get('gastos', [])
    total_ahorro = data.get('totalAhorro', 0)

    # Creamos un texto descriptivo de los gastos para la IA
    detalles_gastos = ", ".join(
        [f"{g['concepto']} (${g['montoAhorro']})" for g in gastos_malos])

    # El "Prompt": Aquí es donde le damos la personalidad a la IA
    prompt = f"""
    Actúa como un coach financiero experto y sarcástico pero motivador. 
    El usuario ha registrado los siguientes gastos que él mismo clasificó como malos o evitables: {detalles_gastos}.
    El ahorro total potencial es de ${total_ahorro}.
    
    Escribe un consejo breve (OJO: MAXIMO 3 LINEAS) que analice estos gastos específicos. 
    Si hay comida chatarra, menciona la salud. Si hay suscripciones, menciona el olvido.
    Dile cuánto ahorraría al año si multiplica esos ${total_ahorro} por 12.
    """

    try:
        response = model.generate_content(prompt)
        consejo_ia = response.text
    except Exception as e:
        print(f"Error detectado en Gemini: {e}")
        consejo_ia = f"¡Vaya! Mi cerebro de IA se trabó, pero ahorrar ${total_ahorro} siempre es buena idea."

    return jsonify({
        'mensajeMotivacional': consejo_ia,
        'ahorroAnual': total_ahorro * 12
    })


if __name__ == '__main__':
    app.run(host='0.0.0.0', port=8080)
