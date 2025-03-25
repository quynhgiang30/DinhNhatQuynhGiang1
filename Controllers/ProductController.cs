//using DinhNhatQuynhGiang1.Models;
//using DinhNhatQuynhGiang1.Models.Repository;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.Rendering;
//using System.Threading.Tasks;

//namespace _2280600958_NguyenTranTrungHieu_3.Controllers
//{
//    //[Area("Admin")]
//    //[Authorize(Roles = "Admin")]
//    public class ProductController : Controller
//    {


//        private readonly IProductRepository _productRepository;
//        private readonly ICategoryRepository _categoryRepository;

//        public ProductController(IProductRepository productRepository, ICategoryRepository categoryRepository)
//        {
//            _productRepository = productRepository;
//            _categoryRepository = categoryRepository;
//        }

//        // Hiển thị danh sách sản phẩm
//        public async Task<IActionResult> Index()
//        {
//            var products = await _productRepository.GetAllAsync();
//            return View(products);
//        }

//        // Hiển thị form thêm sản phẩm
//        public async Task<IActionResult> Add()
//        {
//            var categories = await _categoryRepository.GetAllAsync();
//            ViewBag.Categories = new SelectList(categories, "Id", "Name");
//            return View();
//        }

//        // Xử lý thêm sản phẩm
//        [HttpPost]
//        public async Task<IActionResult> Add(Product product)
//        {
//            if (ModelState.IsValid)
//            {
//                await _productRepository.AddAsync(product);
//                return RedirectToAction(nameof(Index));
//            }

//            // Nếu ModelState không hợp lệ, hiển thị lại form với dữ liệu đã nhập
//            var categories = await _categoryRepository.GetAllAsync();
//            ViewBag.Categories = new SelectList(categories, "Id", "Name");
//            return View(product);
//        }


//        // Hiển thị chi tiết sản phẩm
//        public async Task<IActionResult> Display(int id)
//        {
//            var product = await _productRepository.GetByIdAsync(id);
//            if (product == null)
//            {
//                return NotFound();
//            }
//            return View(product);
//        }

//        // Hiển thị form cập nhật sản phẩm
//        public async Task<IActionResult> Update(int id)
//        {
//            var product = await _productRepository.GetByIdAsync(id);
//            if (product == null)
//            {
//                return NotFound();
//            }

//            var categories = await _categoryRepository.GetAllAsync();
//            ViewBag.Categories = new SelectList(categories, "Id", "Name", product.CategoryId);
//            return View(product);
//        }

//        // Xử lý cập nhật sản phẩm
//        [HttpPost]
//        public async Task<IActionResult> Update(int id, Product product)
//        {
//            if (id != product.Id)
//            {
//                return NotFound();
//            }

//            if (ModelState.IsValid)
//            {
//                await _productRepository.UpdateAsync(product);
//                return RedirectToAction(nameof(Index));
//            }

//            // Nếu ModelState không hợp lệ, hiển thị lại form với dữ liệu đã nhập
//            var categories = await _categoryRepository.GetAllAsync();
//            ViewBag.Categories = new SelectList(categories, "Id", "Name", product.CategoryId);
//            return View(product);
//        }

//        // Hiển thị form xác nhận xóa sản phẩm
//        public async Task<IActionResult> Delete(int id)
//        {
//            var product = await _productRepository.GetByIdAsync(id);
//            if (product == null)
//            {
//                return NotFound();
//            }
//            return View(product);
//        }

//        // Xử lý xóa sản phẩm
//        [HttpPost, ActionName("Delete")]
//        public async Task<IActionResult> DeleteConfirmed(int id)
//        {
//            await _productRepository.DeleteAsync(id);
//            return RedirectToAction(nameof(Index));
//        }
//    }

//}