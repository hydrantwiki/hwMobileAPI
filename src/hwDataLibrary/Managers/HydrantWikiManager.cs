﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using HydrantWiki.Library.DAOs;
using HydrantWiki.Library.Enums;
using HydrantWiki.Library.Helpers;
using HydrantWiki.Library.Objects;
using TreeGecko.Library.AWS.Helpers;
using TreeGecko.Library.Common.Helpers;
using TreeGecko.Library.Common.Objects;
using TreeGecko.Library.Geospatial.Enums;
using TreeGecko.Library.Geospatial.Helpers;
using TreeGecko.Library.Geospatial.Objects;
using TreeGecko.Library.Net.Helpers;
using TreeGecko.Library.Net.Interfaces;
using TreeGecko.Library.Net.Managers;
using TreeGecko.Library.Net.Objects;

namespace HydrantWiki.Library.Managers
{
    public class HydrantWikiManager : AbstractCoreManager, IServerDataManager
    {

        public HydrantWikiManager()
            : base("HW")
        {
           
        }

        #region AuthenticationFailures

        public int GetAuthenticationFailureCount(Guid _userGuid, DateTime _since)
        {
            AuthenticationFailureDAO dao = new AuthenticationFailureDAO(MongoDB);
            return dao.GetCountSince(_userGuid, _since.Ticks);
        }

        public void RecordAuthenticationFailure(Guid _userGuid)
        {
            AuthenticationFailureDAO dao = new AuthenticationFailureDAO(MongoDB);

            AuthenticationFailure failure = new AuthenticationFailure
            {
                Guid = Guid.NewGuid(),
                ParentGuid = _userGuid,
                FailureTicks = DateTime.UtcNow.Ticks
            };

            dao.Persist(failure);
        }

        #endregion

        #region ResetFailures

        public int GetResetFailureCount(Guid _userGuid, DateTime _since)
        {
            ResetFailureDAO dao = new ResetFailureDAO(MongoDB);
            return dao.GetCountSince(_userGuid, _since.Ticks);
        }

        public void RecordResetFailure(Guid _userGuid)
        {
            ResetFailureDAO dao = new ResetFailureDAO(MongoDB);

            ResetFailure failure = new ResetFailure
            {
                Guid = Guid.NewGuid(),
                ParentGuid = _userGuid,
                FailureTicks = DateTime.UtcNow.Ticks
            };

            dao.Persist(failure);
        }

        #endregion

        #region Users
        /// <summary>
        /// 
        /// </summary>
        /// <param name="_user"></param>
        public void Persist(User _user)
        {
            UserDAO dao = new UserDAO(MongoDB);
            dao.Persist(_user);
        }

        public TGUser GetUser(Guid _userGuid)
        {
            UserDAO dao =new UserDAO(MongoDB);
            return dao.Get(_userGuid);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_userSource"></param>
        /// <param name="_username"></param>
        /// <returns></returns>
        public User GetUser(string _userSource, string _username)
        {
            UserDAO dao = new UserDAO(MongoDB);
            return dao.Get(_userSource, _username);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<User> GetUsers()
        {
            UserDAO dao = new UserDAO(MongoDB);

            return dao.GetAll();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_userSource"></param>
        /// <param name="_emailAddress"></param>
        /// <returns></returns>
        public User GetUserByEmail(string _userSource, string _emailAddress)
        {
            UserDAO dao = new UserDAO(MongoDB);
            return dao.GetByEmail(_userSource, _emailAddress);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public long GetUserCount()
        {
            return 0;
        }

        #endregion

        #region Tags
        /// <summary>
        /// 
        /// </summary>
        /// <param name="_tagGuid"></param>
        /// <returns></returns>
        public Tag GetTag(Guid _tagGuid)
        {
            TagDAO dao = new TagDAO(MongoDB);
            return dao.Get(_tagGuid);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public int GetTagCount()
        {
            TagDAO dao = new TagDAO(MongoDB);
            return dao.GetTagCount();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_userGuid"></param>
        /// <returns></returns>
        public int GetTagCount(Guid _userGuid)
        {
            TagDAO dao = new TagDAO(MongoDB);
            return dao.GetTagCount(_userGuid);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_userName"></param>
        /// <returns></returns>
        public int GetActiveTagCount(string _userName)
        {
            return 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_userName"></param>
        /// <returns></returns>
        public int GetInactiveTagCount(string _userName)
        {
            return 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_tag"></param>
        public void Persist(Tag _tag)
        {
            TagDAO dao = new TagDAO(MongoDB);
            dao.Persist(_tag);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_hydrantGuid"></param>
        /// <returns></returns>
        public List<Tag> GetTags(Guid _hydrantGuid)
        {
            TagDAO dao = new TagDAO(MongoDB);
            return dao.GetTagsForHydrant(_hydrantGuid);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_userGuid"></param>
        /// <param name="_limit"></param>
        /// <param name="_startKey"></param>
        /// <returns></returns>
        public List<Tag> GetTagsForUser(Guid _userGuid, int _limit, string _startKey)
        {
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_userGuid"></param>
        /// <param name="_page"></param>
        /// <param name="_pageSize"></param>
        /// <returns></returns>
        public List<Tag> GetTagsForUser(Guid _userGuid, int _page, int _pageSize)
        {
            TagDAO dao = new TagDAO(MongoDB);

            var tags = dao.GetTagsForUser(_userGuid, _page, _pageSize);
            return tags;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_userGuid"></param>
        /// <returns></returns>
        public int GetTagCountForUser(Guid _userGuid)
        {
            TagDAO dao = new TagDAO(MongoDB);
            return dao.GetTagCount(_userGuid);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_userGuid"></param>
        /// <returns></returns>
        public List<Tag> GetTagsForUser(Guid _userGuid)
        {
            TagDAO dao = new TagDAO(MongoDB);
            return dao.GetTagsForUser(_userGuid);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<Tag> GetPendingTags()
        {
            TagDAO dao = new TagDAO(MongoDB);
            return dao.GetPendingTags();
        }

        public Tag GetNextPendingTag()
        {
            TagDAO dao = new TagDAO(MongoDB);
            return dao.GetNextPendingTag();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_tag"></param>
        public void DeleteTag(Tag _tag)
        {
            TagDAO dao = new TagDAO(MongoDB);
            dao.Delete(_tag);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_tagGuid"></param>
        public void DeleteTag(Guid _tagGuid)
        {
            TagDAO dao = new TagDAO(MongoDB);
            dao.Delete(_tagGuid);
        }

        #endregion

        #region Hydrant

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_hydrant"></param>
        public void Persist(Hydrant _hydrant)
        {
            HydrantDAO dao = new HydrantDAO(MongoDB);
            dao.Persist(_hydrant);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_hydrant"></param>
        public void DeleteHydrant(Hydrant _hydrant)
        {
            HydrantDAO dao = new HydrantDAO(MongoDB);
            dao.Delete(_hydrant);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_hydrantGuid"></param>
        public void DeleteHydrant(Guid _hydrantGuid)
        {
            HydrantDAO dao = new HydrantDAO(MongoDB);
            dao.Delete(_hydrantGuid);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_hydrantGuid"></param>
        /// <returns></returns>
        public Hydrant GetHydrant(Guid _hydrantGuid)
        {
            HydrantDAO dao = new HydrantDAO(MongoDB);
            return dao.Get(_hydrantGuid);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_hydrant"></param>
        /// <param name="_newPosition"></param>
        /// <param name="_reviewerUserGuid"></param>
        public void UpdateHydrantPosition(Hydrant _hydrant,
            GeoPoint _newPosition, Guid _reviewerUserGuid)
        {
            _hydrant.Position = _newPosition;
            _hydrant.LastReviewerUserGuid = _reviewerUserGuid;
            _hydrant.LastModifiedDateTime = DateTime.UtcNow;

            Persist(_hydrant);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<Hydrant> GetHydrants()
        {
            HydrantDAO dao = new HydrantDAO(MongoDB);
            return dao.GetHydrants();
        }

        #endregion

        #region HydrantImage
        /// <summary>
        /// 
        /// </summary>
        /// <param name="_hydrantImage"></param>
        public void Persist(HydrantImage _hydrantImage)
        {
            HydrantImageDAO dao = new HydrantImageDAO(MongoDB);
            dao.Persist(_hydrantImage);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_hydrantGuid"></param>
        /// <param name="_imageGuid"></param>
        public void RemoveImageFromHydrant(Guid _hydrantGuid, Guid _imageGuid)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_hydrantGuid"></param>
        /// <returns></returns>
        public List<HydrantImage> GetHydrantImages(Guid _hydrantGuid)
        {
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_hydrantGuid"></param>
        public void DeleteHydrantImages(Guid _hydrantGuid)
        {

        }

        public void DeleteHydrantImage(Guid _hydrantGuid, Guid _imageGuid)
        {

        }

        #endregion

        #region Actions
        /// <summary>
        /// 
        /// </summary>
        /// <param name="_tag"></param>
        /// <param name="_reviewerUserGuid"></param>
        /// <returns></returns>
        public Hydrant TagToNewHydrant(Tag _tag, Guid _reviewerUserGuid)
        {
            Hydrant hydrant = new Hydrant
            {
                Active = true,
                CreationDateTime = DateTime.Now,
                OriginalTagDateTime = _tag.DeviceDateTime,
                OriginalTagUserGuid = _tag.UserGuid,
                PrimaryImageGuid = _tag.ImageGuid,
                OriginalReviewerUserGuid = _reviewerUserGuid,
                LastReviewerUserGuid = _reviewerUserGuid,
                Position = _tag.Position
            };
            hydrant.LastModifiedDateTime = hydrant.CreationDateTime;

            foreach (Property property in _tag.Properties)
            {
                hydrant.Properties.SetValue(property.Name, property.Value, property.AttributeType);
            }

            Persist(hydrant);

            //Assign initial image to hydrant
            if (hydrant.PrimaryImageGuid != null)
            {
                AssignImageToHydrant(hydrant.Guid, hydrant.PrimaryImageGuid.Value);
            }

            //Assign tag to hydrant
            _tag.HydrantGuid = hydrant.Guid;

            //Add to Geospatial table
            Persist(_tag);

            return hydrant;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_hydrantGuid"></param>
        /// <param name="_imageGuid"></param>
        public void AssignImageToHydrant(Guid _hydrantGuid, Guid _imageGuid)
        {
            HydrantImage hi = new HydrantImage { 
                                                    HydrantGuid = _hydrantGuid, 
                                                    ImageGuid = _imageGuid 
                                               };

            Persist(hi);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_hydrant"></param>
        /// <param name="_tag"></param>
        /// <param name="_positionAction"></param>
        /// <param name="_reviewerUserGuid"></param>
        public void UpdateHydrantWithNewTag(Hydrant _hydrant,
                                            Tag _tag,
                                            TagPositionAction _positionAction,
                                            Guid _reviewerUserGuid)
        {
            bool needsSaving = false;

            if (_hydrant != null
                && _tag != null)
            {
                if (_tag.ImageGuid != null)
                {
                    if (_hydrant.PrimaryImageGuid == null)
                    {
                        _hydrant.PrimaryImageGuid = _tag.ImageGuid;
                        _hydrant.LastModifiedDateTime = DateTime.UtcNow;
                        _hydrant.LastReviewerUserGuid = _reviewerUserGuid;
                        needsSaving = true;
                    }

                    AssignImageToHydrant(_hydrant.Guid, _tag.ImageGuid.Value);
                }

                if (needsSaving)
                {
                    Persist(_hydrant);
                }
            }
        }

        #endregion

        #region Search

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_latitude"></param>
        /// <param name="_longitude"></param>
        /// <param name="_distance">In Feet</param>
        /// <returns></returns>
        public List<Hydrant> GetHydrants(double _latitude, double _longitude, double _distance)
        {
            return GetHydrants(new GeoPoint(_longitude, _latitude), _distance);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_center"></param>
        /// <param name="_distance">In feet</param>
        /// <returns></returns>
        public List<Hydrant> GetHydrants(GeoPoint _center, double _distance)
        {
            GeoDistance distance = new GeoDistance(DistanceUnits.Feet, _distance);
            GeoBox gb = new GeoBox(_center, distance, distance);
            return GetHydrants(gb);
        }

        public List<Hydrant> GetHydrants(GeoBox _geoBox)
        {
            HydrantDAO dao = new HydrantDAO(MongoDB);
            return dao.GetHydrants(_geoBox);
        }

        public List<Hydrant> GetHydrants(GeoBox _geoBox, int _quantity)
        {
            HydrantDAO dao = new HydrantDAO(MongoDB);
            return dao.GetHydrants(_geoBox, _quantity);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="_tagPosition"></param>
        /// <param name="_distance">Feet</param>
        /// <returns></returns>
        public List<NearbyHydrant> GetNearbyHydrants(GeoPoint _tagPosition, double _distance)
        {
            GeoDistance distance = new GeoDistance(DistanceUnits.Feet, _distance);
            GeoBox gb = new GeoBox(_tagPosition, distance, distance);

            List<Hydrant> hydrants = GetHydrants(gb);

            List<NearbyHydrant> output = new List<NearbyHydrant>();

            foreach (Hydrant hydrant in hydrants)
            {
                NearbyHydrant nbh = new NearbyHydrant
                {
                    Position =  hydrant.Position,
                    Location = string.Format("Latitude: {0}<br>Longitude: {1}",
                        hydrant.Position.Y.ToString("###.######"),
                        hydrant.Position.X.ToString("###.######")),
                    HydrantGuid = hydrant.Guid,
                    DistanceInFeet =
                        PositionHelper.GetDistance(_tagPosition, hydrant.Position).ToFeet().ToString("###.#"),
                    Thumbnail = string.Format("<img src=\"{0}\" class=\"tagimg\" onclick=\"ShowImage('{1}');\">", hydrant.GetUrl(true), hydrant.GetUrl(false)),
                    MatchButton = string.Format("<button type=\"button\" class=\"btn btn-info\" onclick=\"MatchHydrant('{0}')\">Match</button>", hydrant.Guid)
                };

                output.Add(nbh);

            }

            return output;
        }

        #endregion

        #region PasswordReset

        public void Persist(PasswordReset _passwordReset)
        {
            PasswordResetDao dao = new PasswordResetDao(MongoDB);

            //First inactivate any existing
            dao.InactivatePasswordResetRequests(_passwordReset.ParentGuid.Value);

            //Second save the new request
            dao.Persist(_passwordReset);
        }

        public PasswordReset GetPasswordReset(Guid _userGuid, string _code)
        {
            PasswordResetDao dao = new PasswordResetDao(MongoDB);
            return dao.Get(_userGuid, _code);
        }

        #endregion

        #region Install

        public void LogUserToInstall(Guid _installId, string _username)
        {
            InstallDAO dao = new InstallDAO(MongoDB);

            Install install = dao.Get(_installId);
            if (install == null)
            {
                install = new Install
                {
                    Guid = _installId,
                    Usernames = new List<string> { _username }
                };

                dao.Persist(install);
            }
            else
            {
                if (!install.Usernames.Contains(_username))
                {
                    install.Usernames.Add(_username);

                    dao.Persist(install);
                }
            }
        }

        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_username"></param>
        /// <param name="_activeOnly"></param>
        /// <param name="_maxRows"></param>
        /// <param name="_startKey"></param>
        /// <returns></returns>
        public List<TagRow> GetTagRows(string _username, bool _activeOnly, int _maxRows, 
            ref string _startKey)
        {
            return null;
        }

        //Gets all tags
        public List<TagRow> GetTagRows(string _username, bool _activeOnly)
        {
            try
            {
                string startKey = null;

                return GetTagRows(_username, _activeOnly, 0, ref startKey);
            }
            catch (Exception ex)
            {
                TraceFileHelper.Exception(ex);
            }

            return null;
        }

        #region UserStats

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_userStats"></param>
        public void Persist(UserStats _userStats)
        {
            UserStatsDAO dao = new UserStatsDAO(MongoDB);
            dao.Persist(_userStats);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_userStats"></param>
        public void Persist(List<UserStats> _userStats)
        {
            UserStatsDAO dao = new UserStatsDAO(MongoDB);

            foreach (var userStat in _userStats)
            {
                dao.Persist(userStat);
            }            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_userSource"></param>
        /// <param name="_username"></param>
        /// <returns></returns>
        public UserStats GetUserStats(string _userSource, string _username)
        {
            User user = GetUser(_userSource, _username);

            if (user != null)
            {
                return GetUserStats(user.Guid);
            }

            return null;
        }

        public UserStats GetUserStats(Guid _userGuid)
        {
            UserStatsDAO dao = new UserStatsDAO(MongoDB);
            return dao.GetByUser(_userGuid);
        }

        #endregion

        #region DailyStandings

        public DailyStanding GetDailyStanding(DateTime _date)
        {
            DailyStandingDAO dao = new DailyStandingDAO(MongoDB);
            return dao.GetDailyStanding(_date);
        }

        public void Persist(DailyStanding _dailyStanding)
        {
            DailyStandingDAO dao = new DailyStandingDAO(MongoDB);
            dao.Persist(_dailyStanding);
        }

        public List<DailyStandingUser> GetDailyStandingUsers(Guid _dailyStandingGuid)
        {
            DailyStandingUserDAO dao = new DailyStandingUserDAO(MongoDB);
            return dao.GetUserStandings(_dailyStandingGuid);
        }

        public void DeleteUserStandings(Guid _dailyStandingGuid)
        {
            DailyStandingUserDAO dao = new DailyStandingUserDAO(MongoDB);
            dao.DeleteUserStandings(_dailyStandingGuid);
        }

        public void Persist(DailyStandingUser _dailyStandingUser)
        {
            DailyStandingUserDAO dao = new DailyStandingUserDAO(MongoDB);
            dao.Persist(_dailyStandingUser);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_dailyStandingUsers"></param>
        public void Persist(List<DailyStandingUser> _dailyStandingUsers)
        {
            DailyStandingUserDAO dao = new DailyStandingUserDAO(MongoDB);
            _dailyStandingUsers.ForEach(dsu => dao.Persist(dsu));
        }


        #endregion

        public bool ValidateAuthorizationToken(Guid _userGuid, string _authorizationToken)
        {
            throw new NotImplementedException();
        }

        public void SendUserValidationEmail(TGUser _tgUser, TGUserEmailValidation _tgUserEmailValidation)
        {
            
        }

        public bool SendCannedEmail(TGUser _tgUser, 
            string _cannedEmailName, 
            NameValueCollection _additionParameters)
        {
            try
            {
                CannedEmail cannedEmail = GetCannedEmail(_cannedEmailName);

                if (cannedEmail != null)
                {
                    SystemEmail email = new SystemEmail(cannedEmail.Guid);

                    TGSerializedObject tgso = _tgUser.GetTGSerializedObject();
                    foreach (string key in _additionParameters.Keys)
                    {
                        string value = _additionParameters.Get(key);
                        tgso.Add(key, value);
                    }

                    CannedEmailHelper.PopulateEmail(cannedEmail, email, tgso);

                    TreeGecko.Library.Net.Helpers.EmailHelper.SendMessage(email);
                    Persist(email);

                    return true;
                }
                
                TraceFileHelper.Warning("Canned email not found");
            }
            catch (Exception ex)
            {
                TraceFileHelper.Exception(ex);
            }

            return false;
        }


        #region images

        public void PersistOriginal(Guid _fileGuid, string _extension, 
            string _mimetype, byte[] _data)
        {            
            string bucketName = Config.GetSettingValue("S3Bucket_Originals");
            string folderName = Config.GetSettingValue("S3MediaRootFolder");

            string filename = ImageProcessingHelper.GetPath(_fileGuid, folderName, _extension, false);

            var s3 = S3Helper.GetS3();
            S3Helper.PersistFile(s3, bucketName, filename, _mimetype, _data);
        }

        public byte[] GetOriginal(Guid _fileGuid, string _extension)
        {
            string bucketName = Config.GetSettingValue("S3Bucket_Originals");
            string folderName = Config.GetSettingValue("S3MediaRootFolder");

            string filename = ImageProcessingHelper.GetPath(_fileGuid, folderName, _extension, false);

            var s3 = S3Helper.GetS3();
            return S3Helper.GetFile(s3, bucketName, filename);
        }

        public void PersistWebImage(Guid _fileGuid, string _extension,
            string _mimetype, byte[] _data)
        {
            string bucketName = Config.GetSettingValue("S3Bucket_Images");
            string folderName = Config.GetSettingValue("S3MediaRootFolder");

            string filename = ImageProcessingHelper.GetPath(_fileGuid, folderName, _extension, false);

            var s3 = S3Helper.GetS3();
            S3Helper.PersistFile(s3, bucketName, filename, _mimetype, _data);
        }

        public byte[] GetWebImage(Guid _fileGuid, string _extension)
        {
            string bucketName = Config.GetSettingValue("S3Bucket_Images");
            string folderName = Config.GetSettingValue("S3MediaRootFolder");

            string filename = ImageProcessingHelper.GetPath(_fileGuid, folderName, _extension, false);

            var s3 = S3Helper.GetS3();
            return S3Helper.GetFile(s3, bucketName, filename);
        }

        public void PersistThumbnailImage(Guid _fileGuid, string _extension,
            string _mimetype, byte[] _data)
        {
            string bucketName = Config.GetSettingValue("S3Bucket_Images");
            string folderName = Config.GetSettingValue("S3MediaRootFolder");

            string filename = ImageProcessingHelper.GetPath(_fileGuid, folderName, _extension, true);

            var s3 = S3Helper.GetS3();
            S3Helper.PersistFile(s3, bucketName, filename, _mimetype, _data);
        }

        public byte[] GetThumbnailImage(Guid _fileGuid, string _extension)
        {
            string bucketName = Config.GetSettingValue("S3Bucket_Images");
            string folderName = Config.GetSettingValue("S3MediaRootFolder");

            string filename = ImageProcessingHelper.GetPath(_fileGuid, folderName, _extension, true);

            var s3 = S3Helper.GetS3();
            return S3Helper.GetFile(s3, bucketName, filename);
        }

        #endregion
    }
}
