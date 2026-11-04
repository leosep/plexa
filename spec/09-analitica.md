# 09 · Analítica (Analytics)

Ruta: `/analytics` · Elemento **"Analytics"** del menú.

Mide **entrega, calidad y distribución del trabajo**. Es una vista de lectura, sin filtros ni acciones.

![Analítica — KPIs y flujo acumulado](img/analitica.jpeg)

![Analítica — gráficos inferiores](img/analitica-2.jpeg)

---

## 1. Encabezado

- Título **"Analytics"**.
- Subtítulo: *"Throughput, estimate accuracy and workload distribution."*

---

## 2. Tarjetas KPI

| KPI | Ejemplo | Significado |
|---|---|---|
| **Tasks closed** | `3` · `▲ +100% vs prev. 8w` | Tareas cerradas en el periodo y comparación con las 8 semanas previas |
| **Avg cycle time** | `28.0d` · `▲ +0.0d vs prev.` | Tiempo medio desde que la tarea empieza hasta que se cierra |
| **Rework** | `0%` · `▲ +0 pts` | Proporción de trabajo reabierto/rehecho |
| **Delivery / plan** | `—` · `Target 1.00` | Cumplimiento del plan (1.00 = 100%) |
| **Estimate reliability** | `—` · `within ±20%` | Fiabilidad de las estimaciones (dentro de ±20%) |

### Cómo interpretar
- **Tasks closed** sube → buen ritmo; si baja, revisa bloqueos y WIP.
- **Cycle time** alto → el trabajo tarda en fluir (cuellos en Review, dependencias…).
- **Rework > 0** → retrabajo; revisa calidad y criterios de aceptación.
- **Delivery / plan** y **Estimate reliability** requieren datos suficientes; aparecen como `—` cuando no hay base.

---

## 3. Cumulative flow

- Cabecera: *"Share of the 13 tracked tasks by state, last 8 weeks"*.
- Leyenda: **To do 77%**, **In progress 0%**, **Review 0%**, **Completed 23%**.
- Gráfico de área acumulada por semana (`W37`…`W44`).
- Barra inferior con la proporción de cada estado.

**Uso:** ver cómo se acumula el trabajo. Si *To do* crece sin parar, el backlog se está inflando; si *Review* se ensancha, hay un cuello de validación.

---

## 4. Throughput by week

- *"Tasks completed, last 8 weeks"*.
- Barras por semana (`W37`…`W44`).
- En el ejemplo, la actividad se concentra en **W44**.

**Uso:** comparar semanas y detectar caídas de entrega.

---

## 5. Estimate accuracy

- *"Logged vs estimated hours per tag"*.
- Compara horas registradas con estimadas agrupadas por etiqueta.
- Estado vacío observado: **"No completed tasks with both estimated and logged hours yet."**

**Uso:** cuando hay datos, permite calibrar estimaciones por tipo de trabajo (Backend, Frontend, QA…).

---

## 6. Load per member

- *"Size = active tasks assigned"*.
- Burbujas por persona: **LS 10**, **GC 0**, **YE 0**.
- El tamaño representa la carga activa.

**Uso:** detectar desequilibrio de carga de un vistazo.

---

## 7. Rework origin

- *"0 tasks reopened this quarter"*.
- Dona con el número de casos.

**Uso:** si crece, identifica qué tipo de tareas se reabren (calidad, requisitos cambiantes, bugs).

---

## 8. Sprint velocity

- *"Points delivered vs committed"*.
- Estado vacío observado: **"No sprints closed yet."**

**Uso:** cuando hay sprints cerrados, permite comprometer puntos realistas en la planificación.

---

## 9. Cómo usar Analytics en la práctica

| Pregunta | Métrica |
|---|---|
| ¿Estamos entregando más o menos? | Tasks closed, Throughput by week |
| ¿El trabajo tarda demasiado? | Avg cycle time |
| ¿Dónde se atasca? | Cumulative flow (columna que crece) |
| ¿Estamos rehaciendo trabajo? | Rework, Rework origin |
| ¿Estimamos bien? | Estimate reliability, Estimate accuracy |
| ¿La carga está equilibrada? | Load per member + Team workload |
| ¿Cumplimos lo comprometido? | Delivery / plan, Sprint velocity |

---

## 10. Notas

- Los periodos por defecto son **últimas 8 semanas** (flujo, throughput) y **trimestre** (rework).
- Sin sprint activo/cerrado, varias métricas aparecen vacías o en `—`.
- Las cifras dependen de que las tareas se muevan de estado correctamente y de que se registre tiempo.
