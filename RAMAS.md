# Ramas del proyecto – Minimarket Jade

Guía para que cada integrante trabaje en su propia rama y luego se integre el trabajo en común.

---

## 1. Idea general

- **main** (o **master**): rama principal con el código estable que ya está integrado.
- **Una rama por persona o por módulo**: cada quien trabaja en su rama y sube sus cambios sin pisar a los demás. Cuando algo esté listo, se hace merge a `main`.

---

## 2. Nombrar las ramas

Conviene usar un criterio claro:

### Por persona
```
main
dev/alexa
dev/sebastian
dev/lino
dev/joan
```
---

## 3. Crear y usar tu rama

### 3.1 Crear tu rama desde la última `main`

Abre terminal en la carpeta **Proyecto** (donde está el `.git`):

```bash
cd "ruta\a\Proyecto"

# Actualizar main (por si alguien ya integró cambios)
git fetch origin
git checkout main
git pull origin main

# Crear tu rama y cambiarte a ella (cambia NOMBRE por tu rama, ej. dev/Alex)
git checkout -b NOMBRE
```

Ejemplos:
- `git checkout -b dev/Alex`

### 3.2 Trabajar en tu rama

Haces tus cambios, commits y subes **solo tu rama**:

```bash
git add .
git commit -m "Descripción breve de lo que hiciste"
git push -u origin NOMBRE
```

La primera vez usas `-u origin NOMBRE`; después basta con:

```bash
git push
```

### 3.3 Ver en qué rama estás

```bash
git branch
```

La rama actual sale con un `*`.

### 3.4 Cambiar de rama

```bash
git checkout main
git checkout dev/alexa
```

O en Git moderno:

```bash
git switch main
git switch dev/alexa
```

---

## 4. Qué hace cada uno

| Paso | Acción |
|------|--------|
| 1 | Entrar a la carpeta del proyecto y asegurarse de estar en `main` actualizada. |
| 2 | Crear **su propia rama** con el nombre acordado (ej. `dev/Alex`). |
| 3 | Trabajar solo en esa rama: editar, `git add`, `git commit`, `git push origin su-rama`. |
| 4 | No hacer `git push origin main` desde su rama; `main` se actualiza cuando alguien hace **merge** (en GitHub/GitLab o por comando). |

---

## 5. Integrar una rama en `main` (merge)

Cuando una tarea o módulo esté listo para pasar a `main`:

### En GitHub / GitLab (recomendado)

1. Sube tu rama: `git push origin tu-rama`.
2. En el repositorio web, abre **Pull Request** (GitHub) o **Merge Request** (GitLab) desde `tu-rama` hacia `main`.
3. Revisen si quieren, aprueben y hagan **Merge**.

### Por línea de comandos

```bash
git checkout main
git pull origin main
git merge tu-rama
git push origin main
```

Después de integrar, cada uno puede actualizar su rama desde `main`:

```bash
git checkout dev/Alex
git merge main
```

---

## 6. Resumen de comandos por rol

### “Soy yo y quiero mi rama”
```bash
git checkout main
git pull origin main
git checkout -b dev/MI_NOMBRE
# ... trabajar ...
git add .
git commit -m "Mi avance"
git push -u origin dev/MI_NOMBRE
```

### “Quiero ver la rama de un compañero”
```bash
git fetch origin
git checkout rama-del-companero
```

### “Quiero traer los últimos cambios de main a mi rama”
```bash
git checkout dev/MI_NOMBRE
git fetch origin
git merge origin/main
```

---

## 7. Ejemplo de reparto de ramas

| Integrante | Rama sugerida | Módulo / responsabilidad |
|------------|----------------|---------------------------|
| Alexa | `dev/Alex` | Productos / categorías |
| Sebastian | `dev/sebastian` | Ventas |
| Lino | `dev/lino` | Inventario |
| Joan | `dev/joan` | Clientes / reportes |

Pueden copiar esta tabla al README del repo y ajustar nombres y módulos a lo que acuerden.

---

Siempre que duden, eviten hacer `push` directo a `main`; trabajen en su rama y usen Pull/Merge Request para integrar a `main`.
