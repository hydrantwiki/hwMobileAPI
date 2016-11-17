using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using HydrantWiki.Library.Constants;
using HydrantWiki.Library.Managers;
using HydrantWiki.Library.Objects;
using HydrantWiki.Mobile.Api.Extensions;
using HydrantWiki.Mobile.Api.Helpers;
using HydrantWiki.Mobile.Api.Objects;
using HydrantWiki.Mobile.Api.ResponseObjects;
using Nancy;
using Newtonsoft.Json;
using TreeGecko.Library.Common.Helpers;
using TreeGecko.Library.Geospatial.Helpers;
using TreeGecko.Library.Geospatial.Objects;
using TreeGecko.Library.Net.Objects;
using Tag = HydrantWiki.Library.Objects.Tag;
using User = HydrantWiki.Library.Objects.User;
using TreeGecko.Library.Common.Enums;
using HydrantWiki.Library.Helpers;
using System.Collections.Specialized;

namespace HydrantWiki.Mobile.Api.Modules
{
    public class ApiModule: NancyModule
    {
        public ApiModule()
        {
            Get["/"] = _parameters =>
            {
                return "HydrantWiki Api";
            };

            Get["/api/authorize"] = _parameters => Response.AsError(HttpStatusCode.MethodNotAllowed, null);

            Post["/api/authorize"] = _parameters =>
            {
                BaseResponse br = Authorize(_parameters);
                return Response.AsSuccess(br);
            };

            Get["/api/user/validate/{token}"] = _parameters =>
            {
                Response br = ValidateEmail(_parameters);
                return br;
            };

            Get["/api/user/isavailable/{username}"] = _parameters =>
            {
                BaseResponse br = IsAvailable(_parameters);
                return Response.AsSuccess(br);
            };

            Get["/api/user/inuse/{email}"] = _parameters =>
            {
                BaseResponse br = EmailInUse(_parameters);
                return Response.AsSuccess(br);
            };

            Post["/api/user/password"] = _parameters =>
            {
                BaseResponse br = ChangePassword(_parameters);
                return Response.AsSuccess(br);
            };

            Post["/api/user/create"] = _parameters =>
            {
                BaseResponse br = CreateAccount(_parameters);
                return Response.AsSuccess(br);
            };

            Get["/api/tags/count"] = _parameters =>
            {
                BaseResponse br = HangleGetTagCount(_parameters);
                return Response.AsSuccess(br);
            };

            Get["/api/tags/mine/count"] = _parameters =>
            {                
                BaseResponse br = HandleGetMyTagCount(_parameters);
                return Response.AsSuccess(br);
            };

            Post["/api/tag"] = _parameters =>
            {
                BaseResponse br = HandleTagPost(_parameters);
                return Response.AsSuccess(br);
            };

            Post["/api/image/{fileName}"] = _parameters =>
            {
                BaseResponse br = HandleImagePost(_parameters);
                return Response.AsSuccess(br);
            };

            Post["/api/image"] = _parameters =>
            {
                BaseResponse br = HandleImagePost(_parameters);
                return Response.AsSuccess(br);
            };

            Get["/api/hydrants/box/{east:decimal}/{west:decimal}/{north:decimal}/{south:decimal}"] = _parameters =>
            {
                BaseResponse br = HangleGetHydrantsByGeobox(_parameters);
                return Response.AsSuccess(br);
            };

            Get["/api/hydrants/box/{east:decimal}/{west:decimal}/{north:decimal}/{south:decimal}/{quantity:int}"] = _parameters =>
            {
                BaseResponse br = HangleGetHydrantsByGeobox(_parameters);
                return Response.AsSuccess(br);
            };

            Get["/api/hydrants/circle/{latitude:decimal}/{longitude:decimal}/{distance:decimal}"] = _parameters =>
            {
                BaseResponse br = HangleGetHydrantsByCenterDistance(_parameters);
                return Response.AsSuccess(br);
            };

            Get["/api/tags/mine/{page:int}/{pagesize:int}"] = _parameters =>
            {
                BaseResponse br = HandleGetMyTags(_parameters);
                return Response.AsSuccess(br);
            };

            Get["/api/review/tags"] = _parameters =>
            {
                BaseResponse br = HandleGetTagsToReview(_parameters);
                return Response.AsSuccess(br);
            };

            Post["/api/review/tag/{tagid:guid}/approve"] = _parameters =>
            {
                BaseResponse br = HandleApproveTag(_parameters);
                return Response.AsSuccess(br);
            };

            Post["/api/review/tag/{tagid:guid}/reject"] = _parameters =>
            {
                BaseResponse br = HandleRejectTag(_parameters);
                return Response.AsSuccess(br);
            };

            Post["/api/review/tag/{tagid:guid}/match/{hydrantid:guid}"] = _parameters =>
            {
                BaseResponse br = HandleMatchTag(_parameters);
                return Response.AsSuccess(br);
            };
        }

        public Response ValidateEmail(DynamicDictionary _parameters)
        {
            const string success = @"
                <html>
                    <head>
                    </head>
                    <body>
                        <p>You have successfully validated your email with HydrantWiki.</p>
                        <p><a href=""www.hydrantwiki.com"">HydrantWiki</a></p>
                    </body>
                </html>";

            const string failure = @"
                <html>
                    <head>
                    </head>
                    <body>
                        <p>Unable to validate your email with HydrantWiki.</p>
                        <p><a href=""www.hydrantwiki.com"">HydrantWiki</a></p>
                    </body>
                </html>";

            string validationToken = _parameters["token"];

            HydrantWikiManager hwManager = new HydrantWikiManager();

            if (!string.IsNullOrEmpty(validationToken))
            {
                TGUserEmailValidation uev = hwManager.GetTGUserEmailValidation(validationToken);

                if (uev != null
                    && uev.ParentGuid != null)
                {
                    User user = (User)hwManager.GetUser(uev.ParentGuid.Value);

                    if (user != null)
                    {
                        user.IsVerified = true;

                        hwManager.Persist(user);
                        hwManager.Delete(uev);
                        hwManager.LogInfo(user.Guid, string.Format("Validated email address ({0})", user.EmailAddress));

                        Response successResponse = Response.AsText(success);
                        successResponse.ContentType = "text/html";
                        return successResponse;
                    }
                    else
                    {
                        //User not found.
                        hwManager.LogWarning(Guid.Empty, string.Format("User not found (Token:{0})", validationToken));
                    }
                }
                else
                {
                    //Validation text not found in database
                    hwManager.LogWarning(Guid.Empty, string.Format("Validated token not found ({0})", validationToken));
                }
            }
            else
            {
                //Validation text not supplied.
                hwManager.LogWarning(Guid.Empty, "Validation token not supplied");
            }

            Response failureResponse = Response.AsText(failure);
            failureResponse.ContentType = "text/html";
            return failureResponse;
        }

        public BaseResponse CreateAccount(DynamicDictionary _parameters)
        {
            BaseResponse response = new BaseResponse();
            HydrantWikiManager hwm = new HydrantWikiManager();

            try
            {
                string json = Request.Body.ReadAsString();
                Objects.CreateAccount account = JsonConvert.DeserializeObject<Objects.CreateAccount>(json);

                User user = hwm.GetUser(UserSources.HydrantWiki, account.Username);
                if (user == null)
                {
                    user = hwm.GetUserByEmail(UserSources.HydrantWiki, account.Email);
                    if (user == null)
                    {
                        user = new User();
                        user.Guid = Guid.NewGuid();
                        user.Active = true;
                        user.DisplayName = account.Username;
                        user.Username = account.Username;
                        user.EmailAddress = account.Email;
                        user.UserSource = UserSources.HydrantWiki;
                        user.UserType = UserTypes.User;
                        user.IsVerified = false;
                        hwm.Persist(user);

                        TGUserPassword userPassword = TGUserPassword.GetNew(user.Guid, user.Username, account.Password);
                        hwm.Persist(userPassword);

                        TGUserEmailValidation validation = new TGUserEmailValidation(user);
                        hwm.Persist(validation);

                        NameValueCollection nvc = new NameValueCollection
                        {
                            {"SystemUrl", Config.GetSettingValue("SystemUrl")},
                            {"ValidationText", validation.ValidationText }
                        };
                        hwm.SendCannedEmail(user, CannedEmailNames.ValidateEmailAddress, nvc);
                        hwm.LogInfo(user.Guid, "User created");

                        response.Success = true;
                        response.Message = "Please check your email to finish activating your account";
                        return response;
                    }
                    else
                    {
                        response.Success = false;
                        response.Message = "Email already in use.";
                    }
                } else
                {
                    response.Success = false;
                    response.Message = "Username already exists.";
                }

                hwm.LogWarning(Guid.Empty, response.Message);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = "An error occurred";
                response.Error = "An error occurred";
                hwm.LogException(Guid.Empty, ex);
            }

            return response;
        }

        public BaseResponse HandleMatchTag(DynamicDictionary _parameters)
        {
            User user;
            string message = null;
            HydrantWikiManager hwm = new HydrantWikiManager();

            if (AuthHelper.IsAuthorized(Request, out user))
            {
                if (user.UserType == UserTypes.SuperUser
                    || user.UserType == UserTypes.Administrator)
                {
                    Guid? tagId = _parameters["tagid"];
                    Guid? hydrantId = _parameters["hydrantid"];

                    if (tagId != null
                        && hydrantId != null)
                    {
                        Tag tag = hwm.GetTag(tagId.Value);

                        if (tag != null)
                        {
                            Hydrant hydrant = hwm.GetHydrant(hydrantId.Value);

                            if (hydrant != null)
                            {
                                //Set the tag to approved
                                tag.HydrantGuid = hydrant.Guid;
                                tag.Status = TagStatus.Approved;
                                hwm.Persist(tag);

                                //Set the hydrant review
                                hydrant.LastReviewerUserGuid = user.Guid;
                                hwm.Persist(hydrant);

                                //Update the stats
                                Guid userGuid = tag.UserGuid;
                                UserStats stats = hwm.GetUserStats(userGuid);
                                if (stats == null)
                                {
                                    stats = new UserStats();
                                    stats.UserGuid = userGuid;
                                    stats.Active = true;
                                }
                                stats.ApprovedTagCount++;
                                hwm.Persist(stats);

                                hwm.LogInfo(user.Guid, string.Format("Tag Matched ({0} to {1})", tagId, hydrantId));

                                return new ReviewTagResponse { Success = true };
                            }
                            else
                            {
                                message = "Hydrant not found";
                            }
                        }
                        else
                        {
                            message = "Tag not found";
                        }
                    }
                    else
                    {
                        message = "TagId or HydrantId not specified";
                    }
                }

                hwm.LogWarning(user.Guid, string.Format("Tag Match Error ({0})", message));
            }           

            return new ReviewTagResponse { Success = false, Message = message };
        }

        public BaseResponse HandleApproveTag(DynamicDictionary _parameters)
        {
            User user;
            string message = null;
            HydrantWikiManager hwm = new HydrantWikiManager();

            if (AuthHelper.IsAuthorized(Request, out user))
            {
                if (user.UserType == UserTypes.SuperUser
                    || user.UserType == UserTypes.Administrator)
                {
                    Guid? tagId = _parameters["tagid"];

                    if (tagId != null)
                    {                         
                        Tag tag = hwm.GetTag(tagId.Value);

                        if (tag != null)
                        {
                            if (tag.Status == TagStatus.Pending)
                            {
                                //Create hydrant
                                Hydrant hydrant = new Hydrant
                                {
                                    Guid = Guid.NewGuid(),
                                    Active = true,
                                    CreationDateTime = tag.DeviceDateTime,
                                    LastModifiedBy = tag.LastModifiedBy,
                                    LastModifiedDateTime = tag.LastModifiedDateTime,
                                    LastReviewerUserGuid = user.Guid,
                                    OriginalReviewerUserGuid = user.Guid,
                                    OriginalTagDateTime = tag.DeviceDateTime,
                                    OriginalTagUserGuid = tag.UserGuid,
                                    Position = tag.Position,
                                    PrimaryImageGuid = tag.ImageGuid
                                };
                                hwm.Persist(hydrant);

                                //Set the tag to approved
                                tag.Status = TagStatus.Approved;
                                hwm.Persist(tag);

                                //Update the stats
                                Guid userGuid = tag.UserGuid;
                                UserStats stats = hwm.GetUserStats(userGuid);
                                if (stats == null)
                                {
                                    stats = new UserStats();
                                    stats.UserGuid = userGuid;
                                    stats.Active = true;
                                }
                                stats.ApprovedTagCount++;
                                hwm.Persist(stats);

                                hwm.LogInfo(user.Guid, string.Format("Tag Approved ({0})", tagId));

                                return new ReviewTagResponse { Success = true };
                            } else
                            {
                                message = "Tag already reviewed";
                            }
                        } else
                        {
                            message = "Tag not found";
                        }
                    } else
                    {
                        message = "Tag not specified";
                    }
                }

                hwm.LogWarning(user.Guid, string.Format("Tag Approve Error ({0})", message));
            } else
            {
                LogUnauthorized(Request);
            }

            return new ReviewTagResponse { Success = false, Message = message };
        }

        public BaseResponse HandleRejectTag(DynamicDictionary _parameters)
        {
            User user;
            string message = null;
            HydrantWikiManager hwm = new HydrantWikiManager();

            if (AuthHelper.IsAuthorized(Request, out user))
            {
                if (user.UserType == UserTypes.SuperUser
                    || user.UserType == UserTypes.Administrator)
                {
                    Guid? tagId = _parameters["tagid"];

                    if (tagId != null)
                    {
                       
                        Tag tag = hwm.GetTag(tagId.Value);

                        if (tag != null)
                        {
                            //Set the tag to approved
                            tag.Status = TagStatus.Rejected;
                            hwm.Persist(tag);

                            //Update the stats
                            Guid userGuid = tag.UserGuid;
                            UserStats stats = hwm.GetUserStats(userGuid);
                            if (stats == null)
                            {
                                stats = new UserStats();
                                stats.UserGuid = userGuid;
                                stats.Active = true;
                            }
                            stats.RejectedTagCount++;
                            hwm.Persist(stats);

                            hwm.LogInfo(user.Guid, string.Format("Tag Rejected ({0})", tagId));

                            return new ReviewTagResponse { Success = true };
                        } else
                        {
                            message = "Tag not found";
                        }
                    } 
                    else
                    {
                        message = "TagId not specified";
                    }
                }

                hwm.LogWarning(user.Guid, string.Format("Tag Reject Error ({0})", message));
            }
            else
            {
                LogUnauthorized(Request);               
            }

            return new ReviewTagResponse { Success = false, Message = message};
        }

        private BaseResponse HandleGetTagsToReview(DynamicDictionary _parameters)
        {
            User user;

            if (AuthHelper.IsAuthorized(Request, out user))
            {
                if (user.UserType == UserTypes.SuperUser
                    || user.UserType == UserTypes.Administrator)
                {
                    HydrantWikiManager hwm = new HydrantWikiManager();
                    List<Tag> tags = hwm.GetPendingTags();

                    List<TagToReview> tagsToReview = new List<TagToReview>();

                    foreach (var tag in tags)
                    {
                        TagToReview reviewTag = new TagToReview();
                        reviewTag.TagId = tag.Guid;
                        reviewTag.ImageGuid = tag.ImageGuid;

                        TGUser tagUser = hwm.GetUser(tag.UserGuid);
                        if (tagUser != null)
                        {
                            reviewTag.Username = tagUser.Username;

                            UserStats stats = hwm.GetUserStats(tagUser.Guid);
                            reviewTag.UserTagsApproved = stats.ApprovedTagCount;
                            reviewTag.UserTagsRejected = stats.RejectedTagCount;
                        }

                        if (tag.ImageGuid != null)
                        {
                            reviewTag.ThumbnailUrl = tag.GetUrl(true);
                            reviewTag.ImageUrl = tag.GetUrl(false);
                        }

                        if (tag.Position != null)
                        {
                            reviewTag.Position = new Position()
                            {
                                Latitude = tag.Position.Y,
                                Longitude = tag.Position.X
                            };

                            List<Hydrant> nearby = hwm.GetHydrants(
                                reviewTag.Position.Latitude,
                                reviewTag.Position.Longitude,
                                200);

                            reviewTag.NearbyHydrants = ProcessHydrants(nearby, tag.Position);
                        }
                       
                        tagsToReview.Add(reviewTag);
                    }

                    hwm.LogInfo(user.Guid, string.Format("Retrieved Tags to Review ({0})", tagsToReview.Count));

                    return new TagsToReviewResponse()
                    {
                        Success = true,
                        Tags = tagsToReview
                    };
                }
                else
                {
                    return new BaseResponse {
                        Error = "User not allowed to review tags",
                        Success = false
                    };                
                }
            }
            else
            {
                LogUnauthorized(Request);
                return new BaseResponse
                {
                    Error = "Not authenticated",
                    Success = false
                };
            }
        }

        private BaseResponse HandleGetMyTags(DynamicDictionary _parameters)
        {
            User user;
            BaseResponse response;

            if (AuthHelper.IsAuthorized(Request, out user))
            {
                int page = _parameters["page"];
                int pageSize = _parameters["pagesize"];
                
                HydrantWikiManager hwm = new HydrantWikiManager();

                List<Tag> tags = hwm.GetTagsForUser(user.Guid, page, pageSize);
                List<Objects.Tag> outputTags = new List<Objects.Tag>();

                foreach (Tag tag in tags)
                {
                    Objects.Tag outputTag = new Objects.Tag(tag);
                    outputTags.Add(outputTag);
                }

                int count = hwm.GetTagCount(user.Guid);
                int pages = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(count)/Convert.ToDouble(pageSize)));

                response = new TagsResponse(outputTags, page, pageSize, pages, count);
            }
            else
            {
                LogUnauthorized(Request);
                response = new BaseResponse
                {
                    Success = false,
                    Error = "Unauthorized",
                    Message = "Reauthenticate"
                };
            }

            return response;
        }

        private BaseResponse HangleGetTagCount(DynamicDictionary _parameters)
        {
            User user;
            BaseResponse response;

            if (AuthHelper.IsAuthorized(Request, out user))
            {
                HydrantWikiManager hwm = new HydrantWikiManager();
                int count = hwm.GetTagCount();

                response = new TagCountResponse(true, count);
                hwm.LogInfo(user.Guid, "Retrieved Tag Count");
            }
            else
            {
                LogUnauthorized(Request);
                response = new BaseResponse
                {
                    Success = false,
                    Error = "Unauthorized",
                    Message = "Reauthenticate"
                };
            }

            return response;
        }

        private BaseResponse HandleGetMyTagCount(DynamicDictionary _parameters)
        {
            User user;
            BaseResponse response;
            
            if (AuthHelper.IsAuthorized(Request, out user))
            {
                HydrantWikiManager hwm = new HydrantWikiManager();
                int count = hwm.GetTagCount(user.Guid);

                response = new TagCountResponse(true, count);
                hwm.LogInfo(user.Guid, "Retrieved My Tag Count");

            }
            else
            {
                LogUnauthorized(Request);
                response = new BaseResponse
                {
                    Success = false,
                    Error = "Unauthorized",
                    Message = "Reauthenticate"
                };
            }

            return response;
        }

        private BaseResponse ChangePassword(DynamicDictionary _parameters)
        {
            User user;
            BaseResponse response = new BaseResponse();

            if (AuthHelper.IsAuthorized(Request, out user))
            {
                HydrantWikiManager hwm = new HydrantWikiManager();

                string json = Request.Body.ReadAsString();
                Objects.ChangePassword cp = JsonConvert.DeserializeObject<Objects.ChangePassword>(json);

                if (cp != null)                    
                {
                    if (cp.Username == user.Username
                        && user.UserSource == UserSources.HydrantWiki)
                    {
                        if (hwm.ValidateUser(user, cp.ExistingPassword))
                        {
                            TGUserPassword userPassword = TGUserPassword.GetNew(user.Guid, user.Username, cp.NewPassword);
                            hwm.Persist(userPassword);

                            response.Message = "Password changed";
                            response.Success = true;

                            hwm.LogInfo(user.Guid, response.Message);
                        }
                        else
                        {
                            //Existing password doesn't match
                            response.Message = "Existing password does not match";
                            response.Success = false;

                            hwm.LogWarning(user.Guid, response.Message);
                        }
                    }
                    else
                    {
                        //Usernames don't match
                        response.Message = "Mismatched user";
                        response.Success = false;

                        hwm.LogWarning(user.Guid, response.Message);
                    }
                } else
                {
                    //no body
                    response.Message = "No body";
                    response.Success = false;

                    hwm.LogWarning(user.Guid, response.Message);
                }
            }
            else
            {
                LogUnauthorized(Request);
                response.Message = "Unauthorized";
                response.Success = false;
            }

            return response;
        }


        private BaseResponse Authorize(
            DynamicDictionary _parameters)
        {
            User user;
            BaseResponse response = AuthHelper.Authorize(Request, out user);
            return response;
        }

        private BaseResponse IsAvailable(DynamicDictionary _parameters)
        {
            HydrantWikiManager hwm = new HydrantWikiManager();
            IsAvailableResponse response = new IsAvailableResponse {Available = false, Success = true};

            string username = _parameters["username"];

            if (username != null)
            {
                User user = hwm.GetUser(UserSources.HydrantWiki, username);

                TraceFileHelper.Info("Check if username exists ({0})", username);

                if (user == null)
                {
                    response.Available = true;
                }
            }

            return response;
        }

        private BaseResponse EmailInUse(DynamicDictionary _parameters)
        {
            HydrantWikiManager hwm = new HydrantWikiManager();
            IsAvailableResponse response = new IsAvailableResponse { Available = false, Success = true };

            string email = _parameters["email"];
            if (email != null)
            {
                User user = hwm.GetUserByEmail(UserSources.HydrantWiki, email);

                TraceFileHelper.Info("Check if email in use ({0})", email);

                if (user == null)
                {
                    response.Available = true;
                }
            }

            return response;
        }


        private BaseResponse HandleImagePost(DynamicDictionary _parameters)
        {
            HydrantWikiManager hwManager = new HydrantWikiManager();

            var response = new BaseResponse { Success = false };
            User user;

            if (AuthHelper.IsAuthorized(Request, out user))
            {
                try
                {
                    byte[] fileData = null;
                    string fileName = null;

                    if (Request.Files.Any())
                    {
                        HttpFile file = Request.Files.First();

                        long length = file.Value.Length;
                        fileData = new byte[(int)length];
                        file.Value.Read(fileData, 0, (int)length);
                        fileName = file.Name;
                    }

                    if (fileName != null)
                    {
                        string tempGuid = Path.GetFileNameWithoutExtension(fileName);
                        Guid imageGuid;

                        if (Guid.TryParse(tempGuid, out imageGuid))
                        {

                            hwManager.PersistOriginal(imageGuid, ".jpg", "image/jpg", fileData);
                            hwManager.LogVerbose(user.Guid, "Tag Image Saved");

                            Image original = ImageHelper.GetImage(fileData);

                            fileData = ImageHelper.GetThumbnailBytesOfMaxSize(original, 800);
                            hwManager.PersistWebImage(imageGuid, ".jpg", "image/jpg", fileData);

                            fileData = ImageHelper.GetThumbnailBytesOfMaxSize(original, 100);
                            hwManager.PersistThumbnailImage(imageGuid, ".jpg", "image/jpg", fileData);

                            hwManager.LogInfo(user.Guid, string.Format("Saved Image ({0})", imageGuid));

                            response.Success = true;
                            return response;
                        }
                    }
                }
                catch (Exception ex)
                {
                    TraceFileHelper.Error("Failure to post image");
                    hwManager.LogException(user.Guid, ex);
                }
            }
            else
            {
                LogUnauthorized(Request);
                response.Error = "Unauthorized";
                response.Message = "Reauthenticate";
            }

            return response;
        }

        private void LogUnauthorized(Request _request)
        {
            string username = _request.Headers["Username"].First();
            TraceFileHelper.Warning("{0} - {1} ({2})", _request.UserHostAddress, _request.Url, username);
        }

        private BaseResponse HandleTagPost(DynamicDictionary _parameters)
        {
            HydrantWikiManager hwManager = new HydrantWikiManager();

            var response = new TagPostResponse { Success = false };
            User user;

            if (AuthHelper.IsAuthorized(Request, out user))
            {
                try
                {
                    string json = Request.Body.ReadAsString();

                    Objects.Tag tag = JsonConvert.DeserializeObject<Objects.Tag>(json);

                    if (tag != null)
                    {
                        if (tag.Position != null)
                        {
                            if (tag.Position != null)
                            {
                                var dbTag = new HydrantWiki.Library.Objects.Tag
                                {
                                    Active = true,
                                    DeviceDateTime = tag.Position.DeviceDateTime,
                                    LastModifiedDateTime = DateTime.UtcNow,
                                    UserGuid = user.Guid,
                                    VersionTimeStamp = DateTime.UtcNow.ToString("u"),
                                    Position = new GeoPoint(tag.Position.Longitude, tag.Position.Latitude),
                                    ImageGuid = tag.ImageGuid,
                                    Status = TagStatus.Pending
                                };

                                hwManager.Persist(dbTag);
                                hwManager.LogVerbose(user.Guid, "Tag Saved");

                                response.ImageUrl = dbTag.GetUrl(false);
                                response.ThumbnailUrl = dbTag.GetUrl(true);

                                response.Success = true;
                                return response;

                            }
                            else
                            {
                                //No position
                                hwManager.LogWarning(user.Guid, "No position");

                                response.Message = "No position";
                                return response;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    TraceFileHelper.Error("Failure to post tag");
                    hwManager.LogException(user.Guid, ex);
                }
            }
            else
            {
                LogUnauthorized(Request);
                response.Error = "Unauthorized";
                response.Message = "Reauthenticate";
            }

            return response;
        }

        public BaseResponse HangleGetHydrantsByCenterDistance(DynamicDictionary _parameters)
        {
            var response = new BaseResponse { Success = false };
            User user;

            if (AuthHelper.IsAuthorized(Request, out user))
            {
                double latitude = Convert.ToDouble((string)_parameters["latitude"]);
                double longitude = Convert.ToDouble((string)_parameters["longitude"]);
                double distance = Convert.ToDouble((string)_parameters["distance"]);

                HydrantWikiManager hwm = new HydrantWikiManager();
                GeoPoint center = new GeoPoint(longitude, latitude);

                List<Hydrant> hydrants = hwm.GetHydrants(center, distance);

                List<HydrantHeader> headers = ProcessHydrants(hydrants, center);

                response = new HydrantQueryResponse { Success = true, Hydrants = headers };

                hwm.LogInfo(user.Guid, "Retrieved Hydrants by Center and Distance");

                return response;
            } else
            {
                LogUnauthorized(Request);
            }

            return response;
        }

        public BaseResponse HangleGetHydrantsByGeobox(DynamicDictionary _parameters)
        {
            var response = new BaseResponse { Success = false };
            User user;

            if (AuthHelper.IsAuthorized(Request, out user))
            {
                HydrantWikiManager hwm = new HydrantWikiManager();

                double east = Convert.ToDouble((string)_parameters["east"]);
                double west = Convert.ToDouble((string)_parameters["west"]);
                double north = Convert.ToDouble((string)_parameters["north"]);
                double south = Convert.ToDouble((string)_parameters["south"]);

                int quantity = 250;
                if (_parameters.ContainsKey("quantity"))
                {
                    quantity = Convert.ToInt32((string)_parameters["quantity"]);
                }
                if (quantity > 500)
                {
                    quantity = 500;
                }

                GeoBox geobox = new GeoBox(east, west, north, south);

                List<Hydrant> hydrants = hwm.GetHydrants(geobox, quantity);

                List<HydrantHeader> headers = ProcessHydrants(hydrants);

                response = new HydrantQueryResponse { Success = true, Hydrants = headers };

                hwm.LogInfo(user.Guid, "Retrieved Hydrants by Geobox");

                return response;
            } else
            {
                LogUnauthorized(Request);
            }

            return response;
        }

        private List<HydrantHeader> ProcessHydrants(IEnumerable<Hydrant> _hydrants, GeoPoint _center = null)
        {
            HydrantWikiManager hwm = new HydrantWikiManager();
            Dictionary<Guid, string> users = new Dictionary<Guid, string>();

            var output = new List<HydrantHeader>();
            foreach (var hydrant in _hydrants)
            {
                string username;
                Guid userGuid = hydrant.OriginalTagUserGuid;

                if (users.ContainsKey(userGuid))
                {
                    username = users[userGuid];
                }
                else
                {
                    TGUser user = hwm.GetUser(userGuid);
                    users.Add(user.Guid, user.Username);
                    username = user.Username;
                }

                var outputHydrant = new HydrantHeader
                {
                    HydrantGuid = hydrant.Guid,
                    Position = new GeoLocation(hydrant.Position.Y, hydrant.Position.X, 0),
                    ThumbnailUrl = hydrant.ThumbnailUrl,
                    ImageUrl = hydrant.ImageUrl,
                    Username = username
                };

                if (_center == null)
                {
                    outputHydrant.DistanceInFeet = null;
                }
                else
                {
                    outputHydrant.DistanceInFeet = PositionHelper.GetDistance(_center, hydrant.Position).ToFeet();
                }

                output.Add(outputHydrant);
            }

            return output;
        }
    }
}