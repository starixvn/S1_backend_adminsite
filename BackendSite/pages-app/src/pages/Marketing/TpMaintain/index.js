import React, { useState, useEffect } from "react";
import ReactDOM from "react-dom";
import Result from "./Result";
import WebAPI from "../../../module//WebAPI";
import EditPanel from "./Edit";
import moment from "moment";
import qs from "qs";
import Filter from "./Filter";

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
  
  const dateformat = "YYYY-MM-DD";
  const [stateFilter, setFilter] = useState({
    SelectSite: [],
    StartDate: moment().add(-10, "days").format(dateformat),
    EndDate: moment().format(dateformat),
  });

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

  const SetTpMaintain = (datas, tpGameTypeList, isAdd) => {
    let SetData = {
      siteId: datas.siteId,
      tpId: datas.tpId,
      beginDate: moment(datas.beginDate).toJSON(),
      endDate: moment(datas.endDate).toJSON(),
      maintainType: "Date",
      tpGameTypeList: tpGameTypeList,
      sn: datas.sn
    };
   
    let param = {
      Url: isAdd ? "TpMaintain/InsertTpMaintain" : "TpMaintain/UpdateTpMaintain",
      Data: qs.stringify(SetData),
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

  const EditTpMaintain = (idx) => {
      alert("Only can edit time column.");
      updateDatas({
        ...stateDatas,
        ShowSettingPanel: true,
        SettingDatas: stateDatas.Datas[idx],
        IsAdd: false,
      });    
  };

  const Filter_CheckBox_OnChange = (v, dataColumn) => {
    let isRemove = stateFilter[dataColumn].includes(v);
    let newChecked = isRemove
      ? stateFilter[dataColumn].filter((value) => value !== v)
      : stateFilter[dataColumn].push(v);
    let currentTab = stateFilter[dataColumn];
    if (newChecked.length === 0) {
      currentTab = [];
    } else if (currentTab === null) {
      currentTab = newChecked;
    }

    if (isRemove && !newChecked.includes(currentTab)) {
      currentTab = newChecked.length === 0 ? [] : newChecked;
    }
    setFilter({
      ...stateFilter,
      [dataColumn]: currentTab,
    });
  };

  const Filter_CheckBox_CheckAll = (v, dataColumn, options) => {
    console.log(`all:${v}`);
    if (v !== "ALL") return false;
    let currentTab =
      stateFilter[dataColumn].length === options.length
        ? []
        : options.map((o) => {
            return o.value;
          });
    setFilter({
      ...stateFilter,
      [dataColumn]: currentTab,
    });
  };

  const Filter_OnChange = (v, dataColumn) => {
    setFilter({
      ...stateFilter,
      [dataColumn]: v,
    });
  };

  const Filter_Search= () => {
    alert("search");
  };


  return (
    <div className="container-fluid">
      <h4 className="page-name">{`${
        stateDatas.ShowSettingPanel ? (stateDatas.IsAdd ? "Add " : "Edit ") : ""
      }Maintenance`} </h4>
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
          Datas={stateDatas.SettingDatas}
          IsAdd={stateDatas.IsAdd}
          SetTpMaintain={SetTpMaintain}
        />
      ) : (
        <>
          <Filter
            filterInfo={stateFilter}
            SiteList={siteList}

            onChange={Filter_OnChange}
            onChangeCheckBox={Filter_CheckBox_OnChange}
            onChangeAllCheckBox={Filter_CheckBox_CheckAll}
            ClickSearch={Filter_Search}
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
          Add Maintenance
          </button>
          {stateDatas.Datas === null ? null : (
            <Result
              Datas={stateDatas.Datas}
              ClickDelete={DeleteTpMaintain}
              ClickEdit={EditTpMaintain}
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
