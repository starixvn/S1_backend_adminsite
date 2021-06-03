import React, { useState, useEffect } from "react";
import ReactDOM from "react-dom";
import Result from "./Result";
import WebAPI from "../../../module//WebAPI";
import EditPanel from "./Edit";
import moment from "moment";
import qs from "qs";

const defaultModel = {
  SN: "",
  SiteId: "",
  TpId: "",
  BeginDate: "",
  EndDate: "",
};

const App = () => {
  const [stateDatas, updateDatas] = useState({
    Datas: null,
    SettingDatas: null,
    ShowSettingPanel: false,
    IsAdd: false,
  });

  const [siteList, setSiteList] = useState([]);
  const [tpList, setTpList] = useState([]);

  useEffect(() => {
    let param = {
      Url: "TpMaintain/GetTpMaintainList",
      Data: `PageNumber=${1}`,
    };

    WebAPI.PostData(param).then((res) => {
      if (res.data.errorCode === 0) {
        updateDatas({
          Datas: [...res.data.data],
        });
      } else {
        alert(res.data.message ?? "failed to get data.");
      }
    });

    WebAPI.PostData({ Url: "Common/AdminUserSiteList" }).then((res) => {
      if (res.data.errorCode === 0) {
        let _siteList = {};
        res.data.data.forEach((s) => {
          _siteList[s.siteId] = s.siteName;
        });
        setSiteList(_siteList);
      } else {
        alert(res.data.message ?? "failed to get data.");
      }
    });

    WebAPI.PostData({ Url: "TpMaintain/GetCommonTpList" }).then((res) => {
      if (res.data.errorCode === 0) {
        let _tpList = {};
        res.data.data.forEach((s) => {
          _tpList[s.id] = s.tpName;
        });
        setTpList(_tpList);
      } else {
        alert(res.data.message ?? "failed to get data.");
      }
    });
  }, []);


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
      } else {
        alert(res.data.message ?? "failed to get data.");
      }
    });
  };

  const DeleteTpMaintain = (idx) => {
    if (
      window.confirm(
        `Delete Maintenance which SN is ${stateDatas.Datas[idx].sn} ?`
      )
    ) {
      let param = {
        Url: "TpMaintain/DeleteTpMaintain",
        Data: `sn=${stateDatas.Datas[idx].sn}`,
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
    let param = {
      Url: "TpMaintain/GetTpMaintainList",
      Data: `PageNumber=${PageNumber}`,
    };

    WebAPI.PostData(param).then((res) => {
      if (res.data.errorCode === 0) {
        updateDatas({
          Datas: [...res.data.data],
        });
      } else {
        alert(res.data.message ?? "failed to get data.");
      }
    });
  };

  return (
    <div className="container-fluid">
      <h4 className="page-name">Maintenance</h4>
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
            Add Maintenance
          </button>
          {stateDatas.Datas === null ? null : (
            <Result
              Datas={stateDatas.Datas}
              ClickDelete={DeleteTpMaintain}
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
