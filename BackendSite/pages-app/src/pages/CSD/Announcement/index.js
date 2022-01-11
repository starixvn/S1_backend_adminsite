import React, { useState, useEffect } from "react";
import ReactDOM from "react-dom";
import Result from "./Result";
import WebAPI from "../../../module//WebAPI";
import EditPanel from "./Edit";
import moment from "moment-timezone";
import qs from "qs";
import Filter from "./Filter";
import { api } from "../../../module/WebAPI";

const defaultModel = {
  startTime: "",
  endTime: "",
  subject: {},
  content: {},
  status: 0,
  isTest: 0,
};

const App = () => {
  moment.tz.setDefault("Asia/Taipei");
  const [stateDatas, updateDatas] = useState({
    Datas: null,
    PageInfo: null,
    SettingDatas: null,
    ShowSettingPanel: false,
    IsAdd: false,
  });

  const [siteList, setSiteList] = useState([]);
  const [filterList, setFilterSiteList] = useState([]);

  const [stateFilter, setFilter] = useState({
    SelectSite: [],
    StartDate: moment().add(-10, "days").format("l"),
    EndDate: moment().format("l"),
    Status: 0,
    DateQuickPicker: 0,
  });
  const [sumbitFilter, setsubmitFilter] = useState({
    SelectSite: [],
    StartDate: "",
    EndDate: "",
    Status: 0,
  });

  useEffect(() => {
    WebAPI.PostData({ Url: "Common/AdminUserSiteList" }).then((res) => {
      if (res.data.errorCode === 0) {
        let _siteList = {};
        res.data.data.forEach((s) => {
          _siteList[s.siteId] = s;
        });
        setSiteList(_siteList);
        setFilterSiteList(res.data.data);
      } else {
        alert(res.data.message ?? "failed to get data.");
      }
    });
  }, []);

  const EditAnnouncement = (idx) => {
    let param = {
      Url: "Message/GetAnnouncementById",
      Data: `Id=${stateDatas.Datas[idx].id}`,
    };

    WebAPI.PostData(param).then((res) => {
      if (res.data.errorCode === 0) {
        updateDatas({
          ...stateDatas,
          SettingDatas: {
            ...res.data.data,
          },
          CurrentIdx: idx,
          ShowSettingPanel: true,
          IsAdd: false,
        });
      } else {
        alert(res.data.message ?? "failed to get data.");
      }
    });
  };

  const SetAnnouncement = (datas, isAdd) => {
    let newDatas = {};
    Object.keys(datas).forEach((key) => {
      newDatas[key.charAt(0).toUpperCase() + key.slice(1)] = datas[key];
    });

    let lastUpdTime = datas.lastUpdater ? datas.lastUpdateTime : new Date();

    let subject = {};
    Object.keys(datas.subject).forEach((k) => {
      if (datas.subject[k] !== "") {
        subject[k] = datas.subject[k];
      }
    });

    let content = {};
    Object.keys(datas.content).forEach((k) => {
      if (datas.content[k] !== "") {
        content[k] = datas.content[k];
      }
    });

    let param = {
      Url: isAdd ? "Message/AddAnnouncement" : "Message/UpdateAnnouncement",
      Data: qs.stringify({
        Datas: {
          ...newDatas,
          Seq: datas.seq,
          Subject: JSON.stringify(subject),
          Content: JSON.stringify(content),
          StartTime: moment(datas.startTime).toJSON(),
          EndTime: moment(datas.endTime).toJSON(),
          LastUpdateTime: moment(lastUpdTime).toJSON(),
          CreateTime: moment(datas.createTime).toJSON(),
        },
        _StartTime: moment(datas.startTime).unix(),
        _EndTime: moment(datas.endTime).unix(),
      }),
    };

    WebAPI.PostData(param).then((res) => {
      if (res.data.errorCode === 0) {
        if(stateDatas.Datas != null){
          let tmpDatas = [...stateDatas.Datas];
          let appendDatas = isAdd
            ? {
                creator: res.data.data.creator,
                createTime: res.data.data.createTime,
                id: res.data.data.id,
              }
            : {
                lastUpdater: res.data.data.lastUpdater,
                lastUpdateTime: res.data.data.lastUpdateTime,
              };

          if (isAdd) {
            tmpDatas.push({
              ...datas,
              subject: datas.subject.en,
              content: datas.content.en,
              ...appendDatas,
            });

            tmpDatas = tmpDatas.sort((a, b) => {
              return b.id - a.id;
            });
          } else {
            tmpDatas[stateDatas.CurrentIdx] = {
              ...datas,
              subject: datas.subject.en,
              content: datas.content.en,
              ...appendDatas,
            };
          }

          updateDatas({
            ...stateDatas,
            Datas: tmpDatas,
            SettingDatas: null,
            ShowSettingPanel: false,
          });
        }
        else{
          updateDatas({
            ...stateDatas,
            ShowSettingPanel: false,
          });
        }   
        alert(isAdd ? "Add Success!" : "Update Success!")
      } else {
        alert(res.data.message ?? "failed to get data.");
      }
    });
  };

  const DeleteAnnouncement = (idx) => {
    if (
      window.confirm(
        `Delete Announcement which Id is ${stateDatas.Datas[idx].id} ?`
      )
    ) {
      let param = {
        Url: "Message/DeleteAnnouncement",
        Data: `Id=${stateDatas.Datas[idx].id}`,
      };
      WebAPI.PostData(param).then((res) => {
        if (res.data.errorCode === 0) {
          let tmpDatas = [...stateDatas.Datas];
          tmpDatas.splice(idx, 1);
          updateDatas({
            ...stateDatas,
            Datas: [...tmpDatas],
          });
        } else {
          alert(res.data.message ?? "failed to get data.");
        }
      });
    }
  };

  const Result_ChangePage = (PageNumber) => {
    let Data = {
      SelectSite: sumbitFilter.SelectSite,
      StartDate: sumbitFilter.StartDate,
      EndDate: sumbitFilter.EndDate,
      Status: sumbitFilter.Status,
      PageNumber: PageNumber,
    };
    api
      .post("/api/Message/GetAnnouncement", Data)
      .then(function (res) {
        if (res.data.errorCode === 0) {
          updateDatas({
            PageInfo: {
              totalPages: res.data.totalPages,
              pageNumber: PageNumber,
              rowCount: res.data.rowCount,
            },
            Datas: [...res.data.data],
          });
        } else {
          alert(res.data.message ?? "failed to get data.");
        }
      })
      .catch(function (error) {
        alert(error);
      });
  };

  const Filter_OnChange = (v, dataColumn) => {
    setFilter({
      ...stateFilter,
      [dataColumn]: v,
    });
  };

  const DatePicker_OnChange = (props) => {
    let _startDate = "";
    let _endDate = "";

    switch (props) {
      case "1":
        _startDate = moment().startOf("day");
        _endDate = moment().endOf("day");

        break;
      case "2":
        _startDate = moment().subtract(1, "days");
        _endDate = moment().subtract(1, "days");

        break;
      case "3":
        _startDate = moment().startOf("week");
        _endDate = moment().endOf("week");

        break;
      case "4":
        _startDate = moment().subtract(1, "week").startOf("week");
        _endDate = moment().subtract(1, "week").endOf("week");

        break;
      case "5":
        _startDate = moment().startOf("month");
        _endDate = moment().endOf("month");

        break;
      case "6":
        _startDate = moment().subtract(1, "month").startOf("month");
        _endDate = moment().subtract(1, "month").endOf("month");

        break;
      default:
        break;
    }
    if (props !== "0") {
      setFilter({
        ...stateFilter,
        ["StartDate"]: _startDate.format("l"),
        ["EndDate"]: _endDate.format("l"),
        ["DateQuickPicker"]: props,
      });
    } else {
      setFilter({
        ...stateFilter,
        ["DateQuickPicker"]: props,
      });
    }
  };

  const ClickSubmit = () => {
    if (stateFilter.SelectSite.length === 0) {
      alert("Please Choose at least one Site");
      return false;
    }
    const siteIds = [];
    stateFilter.SelectSite.map((o) => {
      siteIds.push(o.value);
    });
    let Data = {
      SelectSite: siteIds,
      StartDate: moment(stateFilter.StartDate).unix(),
      EndDate: moment(stateFilter.EndDate).add(1, "days").unix() - 1,
      Status: parseInt(stateFilter.Status, 10),
      PageNumber: 1,
    };
    api
      .post("/api/Message/GetAnnouncement", Data)
      .then(function (res) {
        if (res.data.errorCode === 0) {
          updateDatas({
            PageInfo: {
              totalPages: res.data.totalPages,
              pageNumber: 1,
              rowCount: res.data.rowCount,
            },
            Datas: [...res.data.data],
          });
          setsubmitFilter({
            SelectSite: Data.SelectSite,
            StartDate: Data.StartDate,
            EndDate: Data.EndDate,
            Status: Data.Status,
          });
        } else {
          alert(res.data.message ?? "failed to get data.");
        }
      })
      .catch(function (error) {
        alert(error);
      });
  };

  return (
    <div className="container-fluid">
      <h4 className="page-name">{`${
        stateDatas.ShowSettingPanel ? (stateDatas.IsAdd ? "Add " : "Edit ") : ""
      }Announcement`}</h4>
      {stateDatas.ShowSettingPanel ? (
        <EditPanel
          siteList={siteList}
          ClickBack={() => {
            updateDatas({
              ...stateDatas,
              ShowSettingPanel: false,
              IsAdd: false,
            });
          }}
          Datas={stateDatas.SettingDatas}
          IsAdd={stateDatas.IsAdd}
          SetAnnouncement={SetAnnouncement}
        />
      ) : (
        <>
          <Filter
            SiteList={filterList}
            filterInfo={stateFilter}
            onChange={Filter_OnChange}
            datePickeronChange={DatePicker_OnChange}
            ClickSubmit={ClickSubmit}
          />
          <button
            className="btn btn-add"
            style={{ marginBottom: "20px" }}
            onClick={() => {
              updateDatas({
                ...stateDatas,
                SettingDatas: { ...defaultModel },
                ShowSettingPanel: true,
                IsAdd: true,
              });
            }}
          >
            Add Announcement
          </button>
          {stateDatas.Datas === null ? null : (
            <Result
              Datas={stateDatas.Datas}
              PageInfo={stateDatas.PageInfo}
              ClickEdit={EditAnnouncement}
              ClickDelete={DeleteAnnouncement}
              ChangePage={Result_ChangePage}
            />
          )}
        </>
      )}
    </div>
  );
};

export default App;

if (process.env.NODE_ENV === "production") {
  ReactDOM.render(<App />, document.getElementById("content"));
}
