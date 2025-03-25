namespace DinhNhatQuynhGiang1.Models
{
    public class ShoppingCart
    {
        public List<CartItem> Items { get; set; } = new List<CartItem>();
        public void AddItem(CartItem item)
        {
            var existingItem = Items.FirstOrDefault(i => i.ProductId ==
            item.ProductId);
            if (existingItem != null)
            {
                existingItem.Quantity += item.Quantity;
            }
            else
            {
                Items.Add(item);
            }
        }
        public void RemoveItem(int productId)
        {
            var itemToRemove = Items.FirstOrDefault(item => item.ProductId == productId);
            if (itemToRemove != null)
            {
                Items.Remove(itemToRemove);
            }
        }
        //Các phương thức khác
        public decimal GetTotal()
        {
            return Items.Sum(i => i.Price * i.Quantity);
        }
        public void Clear()
        {
            Items.Clear();
        }

    }
}
