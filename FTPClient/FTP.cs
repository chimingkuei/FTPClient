using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace FTPClient
{
    interface IData
    {
        void GetAllDirectory(DirectoryInfo dir, List<DirectoryInfo> list, bool system = false, bool hidden = false);
    }

    class FTP: IData
    {
        public string ftpUser { get; set; }
        public string ftpPassword { get; set; }
        public string ftpRootURL { get; set; }
        public string ip { get; set; }
        public string port { get; set; }

        public FTP(string _ftpUser, string _ftpPassword, string _ip, string _port)
        {
            ftpUser = _ftpUser;
            ftpPassword = _ftpPassword;
            ip = _ip;
            port = _port;
            ftpRootURL = "ftp://" + ip + (string.IsNullOrEmpty(_port) ? "" : ":" + port);
        }

        public bool FileUpload(FileInfo localFile, string ftpPath, string ftpFileName)
        {
            bool success = false;
            FtpWebRequest ftpWebRequest = null;
            FileStream localFileStream = null;
            Stream requestStream = null;
            try
            {
                string uri = ftpRootURL + ftpPath + ftpFileName;
                ftpWebRequest = (FtpWebRequest)FtpWebRequest.Create(new Uri(uri));
                ftpWebRequest.Credentials = new NetworkCredential(ftpUser, ftpPassword);
                ftpWebRequest.UseBinary = true;
                ftpWebRequest.KeepAlive = false;
                ftpWebRequest.Method = WebRequestMethods.Ftp.UploadFile;
                ftpWebRequest.ContentLength = localFile.Length;
                int buffLength = 2048;
                byte[] buff = new byte[buffLength];
                int contentLen;
                localFileStream = localFile.OpenRead();
                requestStream = ftpWebRequest.GetRequestStream();
                contentLen = localFileStream.Read(buff, 0, buffLength);
                while (contentLen != 0)
                {
                    requestStream.Write(buff, 0, contentLen);
                    contentLen = localFileStream.Read(buff, 0, buffLength);
                }
                success = true;
            }
            catch (Exception)
            {
                success = false;
            }
            finally
            {
                if (requestStream != null)
                {
                    requestStream.Close();
                }
                if (localFileStream != null)
                {
                    localFileStream.Close();
                }
            }
            return success;
        }

        public bool FileDownload(string localPath, string localFileName, string ftpPath, string ftpFileName)
        {
            bool success = false;
            FtpWebRequest ftpWebRequest = null;
            FtpWebResponse ftpWebResponse = null;
            Stream ftpResponseStream = null;
            FileStream outputStream = null;
            try
            {
                outputStream = new FileStream(localPath + localFileName, FileMode.Create);
                string uri = ftpRootURL + ftpPath + ftpFileName;
                ftpWebRequest = (FtpWebRequest)FtpWebRequest.Create(new Uri(uri));
                ftpWebRequest.Credentials = new NetworkCredential(ftpUser, ftpPassword);
                ftpWebRequest.UseBinary = true;
                ftpWebRequest.Method = WebRequestMethods.Ftp.DownloadFile;
                ftpWebResponse = (FtpWebResponse)ftpWebRequest.GetResponse();
                ftpResponseStream = ftpWebResponse.GetResponseStream();
                long contentLength = ftpWebResponse.ContentLength;
                int bufferSize = 2048;
                byte[] buffer = new byte[bufferSize];
                int readCount;
                readCount = ftpResponseStream.Read(buffer, 0, bufferSize);
                while (readCount > 0)
                {
                    outputStream.Write(buffer, 0, readCount);
                    readCount = ftpResponseStream.Read(buffer, 0, bufferSize);
                }
                success = true;
            }
            catch (Exception)
            {
                success = false;
            }
            finally
            {
                if (outputStream != null)
                {
                    outputStream.Close();
                }
                if (ftpResponseStream != null)
                {
                    ftpResponseStream.Close();
                }
                if (ftpWebResponse != null)
                {
                    ftpWebResponse.Close();
                }
            }
            return success;
        }

        public bool FileDelete(string ftpPath, string ftpName)
        {
            bool success = false;
            FtpWebRequest ftpWebRequest = null;
            FtpWebResponse ftpWebResponse = null;
            Stream ftpResponseStream = null;
            StreamReader streamReader = null;
            try
            {
                string uri = ftpRootURL + ftpPath + ftpName;
                ftpWebRequest = (FtpWebRequest)FtpWebRequest.Create(new Uri(uri));
                ftpWebRequest.Credentials = new NetworkCredential(ftpUser, ftpPassword);
                ftpWebRequest.KeepAlive = false;
                ftpWebRequest.Method = WebRequestMethods.Ftp.DeleteFile;
                ftpWebResponse = (FtpWebResponse)ftpWebRequest.GetResponse();
                long size = ftpWebResponse.ContentLength;
                ftpResponseStream = ftpWebResponse.GetResponseStream();
                streamReader = new StreamReader(ftpResponseStream);
                string result = String.Empty;
                result = streamReader.ReadToEnd();

                success = true;
            }
            catch (Exception)
            {
                success = false;
            }
            finally
            {
                if (streamReader != null)
                {
                    streamReader.Close();
                }
                if (ftpResponseStream != null)
                {
                    ftpResponseStream.Close();
                }
                if (ftpWebResponse != null)
                {
                    ftpWebResponse.Close();
                }
            }
            return success;
        }

        // 若return false表示檔案不存在
        public bool FileCheckExist(string ftpPath, string ftpName)
        {
            bool success = false;
            FtpWebRequest ftpWebRequest = null;
            WebResponse webResponse = null;
            StreamReader reader = null;
            try
            {
                string url = ftpRootURL + ftpPath;
                ftpWebRequest = (FtpWebRequest)FtpWebRequest.Create(new Uri(url));
                ftpWebRequest.Credentials = new NetworkCredential(ftpUser, ftpPassword);
                ftpWebRequest.Method = WebRequestMethods.Ftp.ListDirectory;
                ftpWebRequest.KeepAlive = false;
                webResponse = ftpWebRequest.GetResponse();
                reader = new StreamReader(webResponse.GetResponseStream());
                string line = reader.ReadLine();
                while (line != null)
                {
                    if (line == ftpName)
                    {
                        success = true;
                        break;
                    }
                    line = reader.ReadLine();
                }
            }
            catch (Exception)
            {
                success = false;
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                }
                if (webResponse != null)
                {
                    webResponse.Close();
                }
            }
            return success;
        }

        // 若return true表示資料夾不存在，則建立其資料夾
        public bool FolderCheckExist(string ftpPath, string ftpDirName)
        {
            bool success = false;
            FtpWebRequest ftpWebRequest = null;
            WebResponse webResponse = null;
            Stream ftpResponseStream = null;
            try
            {
                string url = ftpRootURL + ftpPath + ftpDirName;
                ftpWebRequest = (FtpWebRequest)FtpWebRequest.Create(new Uri(url));
                ftpWebRequest.Credentials = new NetworkCredential(ftpUser, ftpPassword);
                ftpWebRequest.UseBinary = true;
                ftpWebRequest.Method = WebRequestMethods.Ftp.MakeDirectory;
                FtpWebResponse response = (FtpWebResponse)ftpWebRequest.GetResponse();
                ftpResponseStream = response.GetResponseStream();
                success = true;
            }
            catch (Exception)
            {
                success = false;
            }
            finally
            {
                if (ftpResponseStream != null)
                {
                    ftpResponseStream.Close();
                }
                if (webResponse != null)
                {
                    webResponse.Close();
                }
            }
            return success;
        }

        public void GetAllDirectory(DirectoryInfo dir, List<DirectoryInfo> list, bool system = false, bool hidden = false)
        {
            DirectoryInfo[] sub = dir.GetDirectories();
            if (sub.Length > 0)
            {
                list.Add(dir);
            }
            else if (sub.Length == 0)
            {
                list.Add(dir);
                return;
            }
            foreach (DirectoryInfo subDir in sub)
            {
                // 跳过系统目录
                if (system && (subDir.Attributes & FileAttributes.System) == FileAttributes.System)
                    continue;
                // 跳过隐藏目录
                if (hidden && (subDir.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
                    continue;
                GetAllDirectory(subDir, list);
            }
        }

        public IEnumerable<(string, string[], string[])> Walk(string rootPath)
        {
            // 遍歷當前目錄下的所有文件和子目錄         
            string[] files = Directory.GetFiles(rootPath);
            string[] subDirs = Directory.GetDirectories(rootPath);

            yield return (rootPath, subDirs, files);

            // 遞歸遍歷子目錄      
            foreach (string subDir in subDirs)
            {
                foreach (var tuple in Walk(subDir))
                {
                    yield return tuple;
                }
            }
        }


    }


}
