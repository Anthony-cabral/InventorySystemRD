import React, { useEffect, useState } from "react";

const apiUrl = "/api";

const pages = {
  home: "Inicio",
  products: "Productos",
  categories: "Categorías",
  suppliers: "Proveedores",
  movements: "Movimientos de inventario"
};

const names = { products: "producto", categories: "categoría", suppliers: "proveedor" };
const emptyProduct = { name: "", description: "", price: "", stock: "0", categoryId: "", supplierId: "", isActive: true };
const emptyCategory = { name: "", description: "", isActive: true };
const emptySupplier = { name: "", email: "", phoneNumber: "", isActive: true };
const emptyFilters = { search: "", categoryId: "", supplierId: "", isActive: "" };

const errorMessages = {
  "The name is required.": "Ingresa el nombre.",
  "The name cannot exceed 120 characters.": "El nombre admite hasta 120 caracteres.",
  "The description cannot exceed 500 characters.": "La descripción admite hasta 500 caracteres.",
  "This category name is already registered.": "Ya existe una categoría con ese nombre.",
  "A valid email address is required.": "Ingresa un correo electrónico válido.",
  "A valid phone number is required.": "Ingresa un número de teléfono válido.",
  "The category has associated products.": "La categoría tiene productos asociados.",
  "The supplier has associated products.": "El proveedor tiene productos asociados.",
  "The selected category was not found.": "No se encontró la categoría seleccionada.",
  "The selected supplier was not found.": "No se encontró el proveedor seleccionado.",
  "The selected product was not found.": "No se encontró el producto seleccionado.",
  "The selected category or supplier no longer exists.": "La categoría o el proveedor ya no están disponibles.",
  "The stock cannot be negative.": "Las existencias no pueden ser negativas.",
  "The exit quantity exceeds the available stock.": "La salida supera las existencias disponibles.",
  "The resulting stock exceeds the maximum supported quantity.": "Las existencias superarían la cantidad máxima.",
  "The movement quantity must be greater than zero.": "Ingresa una cantidad mayor que cero.",
  "The movement type must be Entry or Exit.": "Selecciona una entrada o una salida.",
  "You need to select a product.": "Selecciona un producto.",
  "The product has inventory movements.": "El producto tiene movimientos de inventario.",
  "The product changed during this operation. Try again.": "El producto cambió. Vuelve a intentarlo.",
  "Stock can only be changed through inventory movements.": "Actualiza las existencias desde Movimientos de inventario.",
  "A price between zero and 9999999999999999.99 with up to two decimal places is required.": "Ingresa un precio válido con hasta dos decimales."
};

function money(value) {
  return new Intl.NumberFormat("es-DO", { style: "currency", currency: "DOP" }).format(value);
}

async function api(path, options = {}) {
  let response;
  try {
    response = await fetch(`${apiUrl}${path}`, {
      ...options,
      headers: options.body ? { "Content-Type": "application/json" } : undefined
    });
  } catch {
    throw new Error("No se pudo conectar con el servidor.");
  }

  if (!response.ok) {
    const message = await response.text();
    throw new Error(errorMessages[message] || "No se pudo completar la operación.");
  }

  if (response.status === 204) return;
  return response.json();
}

function Toast({ message }) {
  return message ? <div className="toast" role="status">{message}</div> : null;
}

function Sidebar({ view, onView }) {
  return (
    <aside className="sidebar">
      <div className="brand"><span className="brand-mark" aria-hidden="true">IR</span><strong>InventoryRD</strong></div>
      <nav className="nav-list" aria-label="Menú principal">
        {Object.entries(pages).map(([key, label]) => (
          <button key={key} type="button" aria-current={view === key ? "page" : undefined} onClick={() => onView(key)}>
            {label}
          </button>
        ))}
      </nav>
    </aside>
  );
}

function Header({ title, right }) {
  return <header className="header"><h1>{title}</h1>{right}</header>;
}

function StatCard({ label, value }) {
  return <div className="stat-card"><span>{label}</span><strong>{value}</strong></div>;
}

function DataTable({ columns, children }) {
  return (
    <div className="table-wrap" tabIndex="0" role="region" aria-label="Tabla de resultados">
      <table>
        <thead><tr>{columns.map(column => <th key={column} scope="col">{column}</th>)}</tr></thead>
        <tbody>{children.length ? children : <tr><td colSpan={columns.length} className="empty">No hay registros para mostrar.</td></tr>}</tbody>
      </table>
    </div>
  );
}

function ConfirmDialog({ data, onCancel, onConfirm, busy }) {
  if (!data) return null;

  return (
    <div className="dialog-backdrop">
      <section className="panel dialog" role="alertdialog" aria-modal="true" aria-labelledby="confirm-title">
        <h2 id="confirm-title">Confirmar eliminación</h2>
        <p>¿Deseas eliminar «{data.name}»?</p>
        <div className="actions">
          <button className="btn muted" type="button" disabled={busy} onClick={onCancel}>Cancelar</button>
          <button className="btn danger" type="button" disabled={busy} onClick={onConfirm}>Confirmar</button>
        </div>
      </section>
    </div>
  );
}

function CatalogForm({ view, form, setForm, categories, suppliers, onSave, onCancel, busy }) {
  const set = event => {
    const { name, value, checked, type } = event.target;
    setForm({ ...form, [name]: type === "checkbox" ? checked : value });
  };

  return (
    <form className="panel editor" onSubmit={onSave}>
      <h2>{form.id ? "Editar" : "Agregar"} {names[view]}</h2>
      <div className="form-grid">
        <label>Nombre<input name="name" value={form.name} onChange={set} maxLength={120} required /></label>
        {view !== "suppliers" && <label>Descripción<textarea name="description" aria-label="Descripción" value={form.description || ""} onChange={set} maxLength={500} /></label>}
        {view === "suppliers" && <>
          <label>Correo electrónico<input name="email" type="email" value={form.email} onChange={set} maxLength={254} required /></label>
          <label>Teléfono<input name="phoneNumber" type="tel" value={form.phoneNumber} onChange={set} maxLength={30} required /></label>
        </>}
        {view === "products" && <>
          <label>Precio<input name="price" type="number" min="0" step="0.01" value={form.price} onChange={set} required /></label>
          <label>Existencias<input name="stock" type="number" min="0" max="2147483647" step="1" value={form.stock} onChange={set} readOnly={Boolean(form.id)} required /></label>
          <label>Categoría
            <select name="categoryId" aria-label="Categoría" value={form.categoryId} onChange={set} required>
              <option value="">Seleccionar</option>
              {categories.map(category => <option key={category.id} value={category.id}>{category.name}</option>)}
            </select>
          </label>
          <label>Proveedor
            <select name="supplierId" aria-label="Proveedor" value={form.supplierId} onChange={set} required>
              <option value="">Seleccionar</option>
              {suppliers.map(supplier => <option key={supplier.id} value={supplier.id}>{supplier.name}</option>)}
            </select>
          </label>
        </>}
        <label className="checkbox"><input name="isActive" type="checkbox" checked={form.isActive} onChange={set} /> Activo</label>
      </div>
      <div className="actions">
        <button className="btn muted" type="button" disabled={busy} onClick={onCancel}>Cancelar</button>
        <button className="btn primary" type="submit" disabled={busy}>Guardar</button>
      </div>
    </form>
  );
}

function CatalogFilters({ view, filters, setFilters, categories, suppliers }) {
  const set = event => setFilters({ ...filters, [event.target.name]: event.target.value });

  return (
    <div className="filters">
      <label>Buscar<input type="search" name="search" value={filters.search} onChange={set} placeholder="Nombre o descripción" /></label>
      {view === "products" && <>
        <label>Categoría
          <select name="categoryId" aria-label="Categoría" value={filters.categoryId} onChange={set}>
            <option value="">Todas</option>
            {categories.map(category => <option key={category.id} value={category.id}>{category.name}</option>)}
          </select>
        </label>
        <label>Proveedor
          <select name="supplierId" aria-label="Proveedor" value={filters.supplierId} onChange={set}>
            <option value="">Todos</option>
            {suppliers.map(supplier => <option key={supplier.id} value={supplier.id}>{supplier.name}</option>)}
          </select>
        </label>
      </>}
      <label>Estado
        <select name="isActive" aria-label="Estado" value={filters.isActive} onChange={set}>
          <option value="">Todos</option>
          <option value="true">Activos</option>
          <option value="false">Inactivos</option>
        </select>
      </label>
    </div>
  );
}

function Home({ products, categories, suppliers }) {
  return (
    <div className="stats-grid">
      <StatCard label="Productos" value={products.length} />
      <StatCard label="Categorías" value={categories.length} />
      <StatCard label="Proveedores" value={suppliers.length} />
      <StatCard label="Unidades disponibles" value={products.reduce((total, product) => total + product.stock, 0)} />
      <StatCard label="Productos activos" value={products.filter(product => product.isActive).length} />
      <StatCard label="Valor del inventario" value={money(products.reduce((total, product) => total + product.stock * product.price, 0))} />
    </div>
  );
}

function CatalogTable({ view, rows, onEdit, onDelete, busy }) {
  const detail = {
    products: ["Categoría", "Proveedor", "Precio", "Existencias"],
    categories: ["Descripción"],
    suppliers: ["Correo electrónico", "Teléfono"]
  };

  return (
    <DataTable columns={["Nombre", ...detail[view], "Estado", "Acciones"]}>
      {rows.map(item => (
        <tr key={item.id}>
          <th scope="row">{item.name}</th>
          {view === "products" && <>
            <td>{item.categoryName}</td><td>{item.supplierName}</td><td>{money(item.price)}</td><td>{item.stock}</td>
          </>}
          {view === "categories" && <td>{item.description || "—"}</td>}
          {view === "suppliers" && <><td>{item.email}</td><td>{item.phoneNumber}</td></>}
          <td><span className={item.isActive ? "badge active" : "badge"}>{item.isActive ? "Activo" : "Inactivo"}</span></td>
          <td>
            <div className="actions">
              <button className="btn muted" type="button" disabled={busy} onClick={() => onEdit(item)}>Editar</button>
              <button className="btn danger" type="button" disabled={busy} onClick={() => onDelete(item)}>Eliminar</button>
            </div>
          </td>
        </tr>
      ))}
    </DataTable>
  );
}

function movementTypeLabel(value) {
  return value === "Entry" ? "Entrada" : "Salida";
}

function dateTimeLabel(value) {
  return new Intl.DateTimeFormat("es-DO", { dateStyle: "short", timeStyle: "short", timeZone: "America/Santo_Domingo" }).format(new Date(value));
}

function MovementHistory({ items }) {
  return (
    <DataTable columns={["Fecha", "Producto", "Tipo", "Cantidad", "Descripción"]}>
      {items.map(item => (
        <tr key={item.id}>
          <td>{dateTimeLabel(item.createdAt)}</td><th scope="row">{item.productName}</th>
          <td><span className={item.movementType === "Entry" ? "badge active" : "badge"}>{movementTypeLabel(item.movementType)}</span></td>
          <td>{item.quantity}</td><td>{item.description || "—"}</td>
        </tr>
      ))}
    </DataTable>
  );
}

function Movements({ products, items, busy, onSave }) {
  const initial = { productId: "", movementType: "Entry", quantity: "", description: "" };
  const [form, setForm] = useState(initial);
  const [filter, setFilter] = useState("");
  const set = event => setForm({ ...form, [event.target.name]: event.target.value });
  const selected = products.find(product => product.id === Number(form.productId));

  const submit = async event => {
    event.preventDefault();
    const saved = await onSave({ ...form, productId: Number(form.productId), quantity: Number(form.quantity) });
    if (saved) setForm({ ...initial, productId: form.productId });
  };

  return (
    <>
      <form className="panel editor" onSubmit={submit}>
        <h2>Registrar movimiento</h2>
        <div className="form-grid">
          <label>Producto
            <select name="productId" aria-label="Producto" value={form.productId} onChange={set} required>
              <option value="">Seleccionar</option>
              {products.map(product => <option key={product.id} value={product.id}>{product.name}</option>)}
            </select>
          </label>
          <label>Tipo
            <select name="movementType" aria-label="Tipo" value={form.movementType} onChange={set}>
              <option value="Entry">Entrada</option><option value="Exit">Salida</option>
            </select>
          </label>
          <label>Cantidad<input type="number" name="quantity" min="1" max="2147483647" step="1" value={form.quantity} onChange={set} required /></label>
          <label>Descripción<textarea name="description" aria-label="Descripción" maxLength={500} value={form.description} onChange={set} /></label>
        </div>
        {selected && <p>Existencias disponibles <strong>{selected.stock}</strong></p>}
        <button className="btn primary" type="submit" disabled={busy || products.length === 0}>Registrar movimiento</button>
      </form>
      <section className="panel">
        <h2>Historial de movimientos</h2>
        <div className="filters">
          <label>Filtrar por producto
            <select aria-label="Filtrar por producto" value={filter} onChange={event => setFilter(event.target.value)}>
              <option value="">Todos</option>
              {products.map(product => <option key={product.id} value={product.id}>{product.name}</option>)}
            </select>
          </label>
        </div>
        <MovementHistory items={items.filter(item => !filter || item.productId === Number(filter))} />
      </section>
    </>
  );
}

export default function App() {
  const [view, setView] = useState("home");
  const [products, setProducts] = useState([]);
  const [categories, setCategories] = useState([]);
  const [suppliers, setSuppliers] = useState([]);
  const [movements, setMovements] = useState([]);
  const [filters, setFilters] = useState(emptyFilters);
  const [form, setForm] = useState(null);
  const [confirm, setConfirm] = useState(null);
  const [message, setMessage] = useState("");
  const [error, setError] = useState("");
  const [busy, setBusy] = useState(false);
  const [loading, setLoading] = useState(true);

  const loadAll = async () => {
    setLoading(true);
    try {
      const [productData, categoryData, supplierData, movementData] = await Promise.all([
        api("/products"), api("/categories"), api("/suppliers"), api("/stockmovements")
      ]);
      setProducts(productData);
      setCategories(categoryData);
      setSuppliers(supplierData);
      setMovements(movementData);
    } catch (exception) {
      setError(exception.message);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { loadAll(); }, []);

  const openView = next => {
    if (busy) return;
    setView(next);
    setFilters(emptyFilters);
    setForm(null);
    setConfirm(null);
    setMessage("");
    setError("");
  };

  const openForm = item => {
    const initial = { products: emptyProduct, categories: emptyCategory, suppliers: emptySupplier };
    setError("");
    setForm(item ? { ...item } : { ...initial[view] });
  };

  const save = async event => {
    event.preventDefault();
    setMessage("");
    setError("");
    setBusy(true);

    const request = { name: form.name, isActive: form.isActive };
    if (form.id) request.id = form.id;
    if (view !== "suppliers") request.description = form.description;
    if (view === "products") {
      request.price = String(form.price);
      if (!form.id) request.stock = Number(form.stock);
      request.categoryId = Number(form.categoryId);
      request.supplierId = Number(form.supplierId);
    }
    if (view === "suppliers") {
      request.email = form.email;
      request.phoneNumber = form.phoneNumber;
    }

    try {
      const path = form.id ? `/${view}/${form.id}` : `/${view}`;
      await api(path, { method: form.id ? "PUT" : "POST", body: JSON.stringify(request) });
      setForm(null);
      await loadAll();
      setMessage("Se guardaron los cambios.");
    } catch (exception) {
      setError(exception.message);
    } finally {
      setBusy(false);
    }
  };

  const deleteItem = async () => {
    setMessage("");
    setError("");
    setBusy(true);
    try {
      await api(`/${view}/${confirm.id}`, { method: "DELETE" });
      setConfirm(null);
      await loadAll();
      setMessage("Se eliminó el registro.");
    } catch (exception) {
      setConfirm(null);
      setError(exception.message);
    } finally {
      setBusy(false);
    }
  };

  const saveMovement = async request => {
    setMessage("");
    setError("");
    setBusy(true);
    try {
      await api("/stockmovements", { method: "POST", body: JSON.stringify(request) });
      await loadAll();
      setMessage("Se registró el movimiento.");
      return true;
    } catch (exception) {
      setError(exception.message);
      return false;
    } finally {
      setBusy(false);
    }
  };

  const records = { products, categories, suppliers };
  const catalog = Boolean(records[view]);
  const search = filters.search.trim().toLocaleLowerCase("es-DO");
  const rows = (records[view] || []).filter(item =>
    (!search || `${item.name} ${item.description || ""} ${item.email || ""}`.toLocaleLowerCase("es-DO").includes(search)) &&
    (!filters.isActive || String(item.isActive) === filters.isActive) &&
    (!filters.categoryId || item.categoryId === Number(filters.categoryId)) &&
    (!filters.supplierId || item.supplierId === Number(filters.supplierId))
  );

  return (
    <>
      <Sidebar view={view} onView={openView} />
      <main className="app-shell">
        <Header title={pages[view]} right={catalog && !form && (
          <button className="btn primary" type="button" disabled={busy} onClick={() => openForm(null)}>Agregar {names[view]}</button>
        )} />
        {error && <div className="error" role="alert">{error}</div>}
        {loading && <p className="loading" role="status">Cargando datos…</p>}
        <Toast message={message} />
        {view === "home" && !loading && <Home products={products} categories={categories} suppliers={suppliers} />}
        {view === "movements" && <Movements products={products} items={movements} busy={busy} onSave={saveMovement} />}
        {catalog && <>
          {form && <CatalogForm view={view} form={form} setForm={setForm} categories={categories} suppliers={suppliers}
            onSave={save} onCancel={() => setForm(null)} busy={busy} />}
          <section className="panel">
            <CatalogFilters view={view} filters={filters} setFilters={setFilters} categories={categories} suppliers={suppliers} />
            <CatalogTable view={view} rows={rows} onEdit={openForm} onDelete={setConfirm} busy={busy} />
          </section>
        </>}
        <ConfirmDialog data={confirm} onCancel={() => setConfirm(null)} onConfirm={deleteItem} busy={busy} />
      </main>
    </>
  );
}
