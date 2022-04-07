import React, { useEffect, useRef, useReducer, useState } from "react";
import Filter from "./Filter";
import Result from "./Result";
import Detail from "./Detail";
import WebAPI from "../../../module/WebAPI";
import qs from "qs";

const App = () => {
  const [siteList, setSiteList] = useState();
  const [filter, setfilter] = useState({
    SiteId: 0,
    Status: 0,
  });
  const [pageStatus, setpageStatus] = useState(0);
  const [bannerList, setbannerList] = useState(null);
  const [bannerData, setbannerData] = useState({
    bannerName: "",
    actionMode: 2,
    showStatus: 3,
    startTime: new Date(),
    endTime: moment(new Date()).add(2, "hour"),
    sportBattledate: new Date(),
    matchId: "",
    tpId: "",
    gameId: "",
    sportLeaguesId: 1,
    home: 0,
    away: 0,
    status: 1,
  });
  const [sportsLeaguesList, setsportsLeaguesList] = useState(null);
  const [sportsTeamsList, setsportsTeamsList] = useState(null);
  const [initsportsTeamsList, setinitsportsTeamsList] = useState(null);
  useEffect(() => {
    let param = {
      Url: "Common/AdminUserSiteList",
    };
    WebAPI.PostData(param).then((res) => {
      if (res.data.errorCode === 0) {
        setSiteList(res.data.data);
      } else {
        alert(res.data.message ?? "failed to get data.");
      }
    });
  }, []);
  useEffect(() => {
    let SportLeaguesListParam = {
      Url: "Sports/GetSportLeaguesList",
      Data: {
        SportCategoryId: 1,
      },
    };
    WebAPI.PostData(SportLeaguesListParam).then((res) => {
      if (res.data.errorCode === 0) {
        setsportsLeaguesList(res.data.data);
        GetSportTeamsList(
          bannerData.sportLeaguesId ? bannerData.sportLeaguesId : 1
        );
      } else {
        alert(res.data.message ?? "failed to get data.");
      }
    });
  }, []);

  useEffect(() => {
    let SportTeamsListParam = {
      Url: "Sports/GetSportTeamsList",
      Data: {
        SportLeaguesId: Number(1),
      },
    };
    WebAPI.PostData(SportTeamsListParam).then((res) => {
      if (res.data.errorCode === 0) {
        setinitsportsTeamsList(res.data.data);
      } else {
        alert(res.data.message ?? "failed to get data.");
      }
    });
  }, []);

  const GetSportTeamsList = (leaguesId) => {
    let SportTeamsListParam = {
      Url: "Sports/GetSportTeamsList",
      Data: {
        SportLeaguesId: Number(leaguesId),
      },
    };
    WebAPI.PostData(SportTeamsListParam).then((res) => {
      if (res.data.errorCode === 0) {
        setsportsTeamsList(res.data.data);
        setHomeAway(res.data.data, leaguesId);
      } else {
        alert(res.data.message ?? "failed to get data.");
      }
    });
  };
  const EditSportTeamsList = (leaguesId) => {
    let SportTeamsListParam = {
      Url: "Sports/GetSportTeamsList",
      Data: {
        SportLeaguesId: Number(leaguesId),
      },
    };
    WebAPI.PostData(SportTeamsListParam).then((res) => {
      if (res.data.errorCode === 0) {
        setsportsTeamsList(res.data.data);
      } else {
        alert(res.data.message ?? "failed to get data.");
      }
    });
  };
  const setHomeAway = (sportsTeamsList, leaguesId) => {
    let SportsTeamsList = sportsTeamsList.map((s) => {
      return {
        value: s.id,
        displayText: s.name,
      };
    });
    setbannerData({
      ...bannerData,
      home: SportsTeamsList[0].value,
      away: SportsTeamsList[0].value,
      sportLeaguesId: leaguesId,
    });
  };

  const changeSportLeaguesId = (value, type) => {
    GetSportTeamsList(value);
  };
  const changeFilter = (value, type) => {
    setfilter({
      ...filter,
      [type]: value,
    });
    if (filter.SiteId != 0) {
      let param = {
        Url: "BannerManage/GetHotEventList",
        Data: {
          SiteId: filter.SiteId,
          Status: type == "Status" ? value : filter.Status,
        },
      };
      WebAPI.PostData(param).then(
        (res) => {
          if (res.data.errorCode === 0) {
            setbannerList(res.data.data);
          } else {
            alert(res.data.message ?? "failed to get data.");
          }
        },
        (err) => {
          alert("API Server Error.");
        }
      );
    }
  };
  const setSiteId = (siteId) => {
    setfilter({
      ...filter,
      SiteId: siteId,
      CurrencyId: 0,
    });
    let param = {
      Url: "BannerManage/GetHotEventList",
      Data: {
        SiteId: siteId,
        Status: filter.Status,
      },
    };
    WebAPI.PostData(param).then(
      (res) => {
        if (res.data.errorCode === 0) {
          setbannerList(res.data.data);
        } else {
          alert(res.data.message ?? "failed to get data.");
        }
      },
      (err) => {
        alert("API Server Error.");
      }
    );
  };
  const UpdateBannerStatus = (idx) => {
    let _banner = bannerList[idx];
    let msg =
      _banner.status === 1 ? "Disable this HotEnent?" : "Enable this HotEnent?";
    if (_banner && window.confirm(msg)) {
      let newStatus = _banner.status === 0 ? 1 : 0;
      let param = {
        Url: "BannerManage/UpdateBannerStatus",
        Data: qs.stringify({
          SiteId: _banner.siteId,
          BannerName: _banner.bannerName,
          Status: newStatus,
        }),
      };
      WebAPI.PostData(param).then((res) => {
        if (res.data.errorCode === 0) {
          GetSiteBannerList();
        } else {
          alert(res.data.message ?? "failed to get data.");
        }
      });
    }
  };
  const GetSiteBannerList = () => {
    let param = {
      Url: "BannerManage/GetHotEventList",
      Data: {
        SiteId: filter.SiteId,
        Status: filter.Status,
      },
    };
    WebAPI.PostData(param).then(
      (res) => {
        if (res.data.errorCode === 0) {
          setbannerList(res.data.data);
        } else {
          alert(res.data.message ?? "failed to get data.");
        }
      },
      (err) => {
        alert("API Server Error.");
      }
    );
  };
  const ClickCreateBanner = () => {
    setpageStatus(1);
  };
  const ClickEdit = (idx) => {
    let param = {
      Url: "BannerManage/GetHotEvent",
      Data: {
        SiteId: bannerList[idx].siteId,
        BannerName: bannerList[idx].bannerName,
      },
    };
    WebAPI.PostData(param).then(
      (res) => {
        if (res.data.errorCode === 0) {
          let bannerdata = res.data.data;
          setbannerData({
            bannerName: bannerdata.bannerName,
            actionMode: bannerdata.actionMode,
            showStatus: bannerdata.showStatus,
            startTime:moment(bannerdata.startTime),
            endTime:moment(bannerdata.endTime),
            sportBattledate:moment(bannerdata.sportBattleDateTime),
            matchId: bannerdata.matchId,
            tpId: bannerdata.tpId,
            gameId: bannerdata.gameId,
            sportLeaguesId: bannerdata.sportLeaguesId,
            home: bannerdata.sportTeamsId[0].home,
            away: bannerdata.sportTeamsId[0].away,
            status: bannerList[idx].status,
          });
          EditSportTeamsList(bannerdata.sportLeaguesId);
          setpageStatus(2);
        } else {
          alert(res.data.message ?? "failed to get data.");
        }
      },
      (err) => {
        alert("API Server Error.");
      }
    );
  };
  const EditBannerData = (value, type) => {
    if (type == "sportBattledate") {
      setbannerData({
        ...bannerData,
        endTime: moment(value).add(2, "hour"),
        sportBattledate: value,
      });
    } else {
      setbannerData({
        ...bannerData,
        [type]: value,
      });
    }
  };
  const ClickBack = () => {
    initbannerData();
    setpageStatus(0);
  };
  const initbannerData = () => {
    let SportsTeamsList = initsportsTeamsList.map((s) => {
      return {
        value: s.id,
        displayText: s.name,
      };
    });
    setsportsTeamsList(initsportsTeamsList);
    setbannerData({
      bannerName: "",
      actionMode: 2,
      showStatus: 3,
      startTime: new Date(),
      endTime: moment(new Date()).add(2, "hour"),
      sportBattledate: new Date(),
      matchId: "",
      tpId: "",
      gameId: "",
      sportLeaguesId: 1,
      home: SportsTeamsList[0].value,
      away: SportsTeamsList[0].value,
      status: 1,
    });
  };
  const ClickSubmit = () => {
    if (Number(bannerData.home) == Number(bannerData.away)) {
      alert("Please choose a different team.");
      return false;
    }
    let _specific = {
      CurrencyId: 0,
      Details: null,
      Status: bannerData.status,
    };
    let _banner = {
      SiteId: Number(filter.SiteId),
      BannerName: bannerData.bannerName,
      BannerType: 2,
      Sort: 0,
      ActionMode: Number(bannerData.actionMode),
      ShowStatus: Number(bannerData.showStatus),
      UnixStartTime: moment(bannerData.startTime).unix(),
      UnixEndTime: moment(bannerData.endTime).unix(),
      BonusCode: null,
      MatchId: Number(bannerData.matchId),
      TpId: Number(bannerData.tpId),
      GameId: bannerData.gameId,
      UrlRouter: null,
      UnixSportBattleDateTime: moment(bannerData.sportBattledate).unix(),
      SportCategoryId: 1,
      SportLeaguesId: Number(bannerData.sportLeaguesId),
      SportTeamsId: [
        {
          Home: Number(bannerData.home),
          Away: Number(bannerData.away),
        },
      ],
      Specific: [_specific],
    };
    let param = {
      Url: "BannerManage/SetBanner",
      Data: qs.stringify({
        bannerInfo: _banner,
      }),
    };
    WebAPI.PostData(param).then(
      (res) => {
        if (res.data.errorCode <= 2) {
          GetSiteBannerList();
          initbannerData();
          setpageStatus(0);
        } else {
          alert(res.data.message ?? "failed to set data.");
        }
      },
      (err) => {
        alert("API Server Error.");
      }
    );
  };
  return (
    <div>
      {pageStatus == 0 ? (
        <>
          <h4 className="detail">Hot Event Management</h4>
          {siteList == null ? null : (
            <Filter
              Site={siteList}
              ChangeFilter={changeFilter}
              Filter={filter}
              SetSiteId={setSiteId}
            />
          )}
          {bannerList == null ? null : (
            <Result
              bannerList={bannerList}
              UpdateBannerStatus={UpdateBannerStatus}
              ClickCreateBanner={ClickCreateBanner}
              ClickEdit={ClickEdit}
            />
          )}
        </>
      ) : null}
      {pageStatus != 0 ? (
        <>
          <Detail
            bannerData={bannerData}
            pageStatus={pageStatus}
            EditBannerData={EditBannerData}
            sportsLeaguesList={sportsLeaguesList}
            sportsTeamsList={sportsTeamsList}
            changeSportLeaguesId={changeSportLeaguesId}
            ClickBack={ClickBack}
            ClickSubmit={ClickSubmit}
          />
        </>
      ) : null}
    </div>
  );
};

export default App;

if (process.env.NODE_ENV === "production") {
  ReactDOM.render(<App />, document.getElementById("content"));
}
