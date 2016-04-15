using System;
using System.Threading.Tasks;

namespace Core.Services
{
	public interface IMediaService
	{
		Task<String> CompressAndResizeImageAsync (String filePath, int desiredWidth, int desiredHeight);

		void DeleteImage (String filePath);
	}
}

