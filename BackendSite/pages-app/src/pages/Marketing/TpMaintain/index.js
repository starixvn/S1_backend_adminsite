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
    ShowSettingPanel: false,
    IsAdd: false,
  });

  const [siteList, setSiteList] = useState([]);
  const [tpList, setTpList] = useState([]);
  const [typeList, setTypeList] = useState([]);
  useEffect(() => {
    GetTpMaintainList();

    WebAPI.PostData({ Url: "Common/AdminUserSiteList" }).then((res) => {
      if (res.data.errorCode === 0) {
        let _siteList = {};
        _siteList[0] = 'ALL';
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
        setTpList(res.data.data);
      } else {
        alert(res.data.message ?? "failed to get data.");
      }
    });

    WebAPI.PostData({ Url: "TpMaintain/GetTpGameTypeList" }).then((res) => {
      if (res.data.errorCode === 0) {    
        setTypeList(res.data.data);
      } else {
        alert(res.data.message ?? "failed to get data.");
      }
    });

  }, []);

  const GetTpMaintainList = () => {
    
    let param = {
      Url: "TpMaintain/GetTpMaintainList"
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

  const InsertTpMaintain = (datas, tpGametype) => {
    let insertData = {
      siteId: datas.siteId,
      tpId: datas.id,
      beginDate: moment(datas.beginDate).toJSON(),
      endDate: moment(datas.endDate).toJSON(),
      maintainType: "Date",
      tpGameTypeList: tpGametype
    };
    
    let param = {
      Url: "TpMaintain/InsertTpMaintain",
      Data: qs.stringify(insertData),
    };
    
    WebAPI.PostData(param).then((res) => {
      if (res.data.errorCode === 0) {
        alert("success.");
        GetTpMaintainList();
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

  const UpdateTpMaintain = (idx) => {
      window.confirm(`SN is ${stateDatas.Datas[idx].sn}`)
  };

  return (
    <div className="container-fluid">
      <h4 className="page-name">Maintenance</h4>
      {stateDatas.ShowSettingPanel ? (
        <EditPanel
          siteList={siteList}
          tpList={tpList}
          typeList={typeList}
          ClickBack={() => {
            updateDatas({
              ...stateDatas,
              ShowSettingPanel: false,
            });
          }}
          InsertTpMaintain={InsertTpMaintain}
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
              ClickEdit={UpdateTpMaintain}
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
