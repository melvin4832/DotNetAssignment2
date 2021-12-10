using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using Lab4.Models;
using Lab4.Data;
using Microsoft.EntityFrameworkCore;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace Lab4.Controllers
{
    public class AdvertisementsController : Controller
    {
        private readonly SchoolCommunityContext _context;
        private readonly BlobServiceClient _blobClient;
        private readonly string blobContainerName = "advert";


        public AdvertisementsController(SchoolCommunityContext context, BlobServiceClient blobClient)
        {
            _context = context;
            _blobClient = blobClient;
        }

        [HttpGet]
        public ActionResult Index(string ID)
        {
            return View(_context.Communities.Include(c => c.Advertisement).First(c => c.Id == ID));
        }

        [HttpGet]
        public ActionResult Create(string ID)
        {
            return View(_context.Communities.First(c => c.Id == ID));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(string ID, IFormFile File)
        {
            if (File == null || File.Length == 0)
            {

                ModelState.AddModelError("", "Invalid File");
                return View(_context.Communities.First(c => c.Id == ID));
            }


            var blobKey = Guid.NewGuid().ToString();

            var con = _blobClient.GetBlobContainerClient(this.blobContainerName);
            con.CreateIfNotExists(Azure.Storage.Blobs.Models.PublicAccessType.Blob);
            var client = con.GetBlobClient(blobKey);
            client.Upload(File.OpenReadStream(), new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders
                {
                    ContentType = File.ContentType
                }
            });
            var Add = new Advertisement
            {
                BlobKey = blobKey,
                Url = client.Uri.AbsoluteUri,
                FileName = File.FileName,
                Community = _context.Communities.First(c => c.Id == ID)
            };
            _context.Advertisements.Add(Add);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index), new { id = ID });
        }

        [HttpGet]
        public ActionResult Delete(int id)
        {
            var Advertisment = _context.Advertisements.Include(c => c.Community).Where(n => n.Id == id).First();
            return View(Advertisment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(Advertisement arg)
        {
            var Advertisment = _context.Advertisements.Include(v => v.Community).First(n => n.Id == arg.Id);

            var c = _blobClient.GetBlobContainerClient(blobContainerName);
            c.DeleteBlobIfExists(Advertisment.BlobKey);

            var communityId = Advertisment.Community.Id;
            _context.Advertisements.Remove(Advertisment);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index), new { id = communityId });
        }
    }
}