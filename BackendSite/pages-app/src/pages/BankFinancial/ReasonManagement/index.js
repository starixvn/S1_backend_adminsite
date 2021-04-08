import React, { useEffect, useRef, useReducer, useState } from "react";
import ReactDOM from "react-dom";
import WebAPI from "../../../module/WebAPI";
import moment from "moment";
import { api } from "../../../module/WebAPI";
import Filter from "./Filter";
import Result from "./Result";
import Detail from "./Detail";

const App = () => {
  const [state, setstate] = useState(0);
  const [siteList, setSiteList] = useState([]);
  const [funckey, setfunckey] = useState(null);
  const [stateFilter, setFilter] = useState({
    Funckey: "0",
  });
  const [stateDatas, setDatas] = useState({
    PageInfo: null,
    Datas: null,
    FilterInfo: null,
  });
  const [editData, seteditData] = useState({
    Id: null,
    FuncKey: null,
    ReasonId: null,
    Description: null,
    Creator: null,
    CreateTime: null,
    Updator: null,
    UpdateTime: null,
  });
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
    api.post("/api/ReasonManagement/GetFuncKey").then((res) => {
      if (res.data.errorCode === 0) {
        console.log(res.data.data);
        setfunckey(res.data.data);
      } else {
        alert(res.data.message ?? "failed to get data.");
      }
    });
  }, []);
  const Filter_OnChange = (v, dataColumn) => {
    setFilter({
      ...stateFilter,
      [dataColumn]: v,
    });
  };
  const Filter_Submit = () => {
    let Data = {
      FuncKey: stateFilter.Funckey,
      PageNumber: parseInt(1),
      PageSize: parseInt(10),
    };
    api
      .post("/api/ReasonManagement/GetReasonManagementData", Data)
      .then(function (res) {
        if (res.data.errorCode === 0) {
          setDatas({
            PageInfo: {
              totalPages: res.data.totalPages,
              pageNumber: 1,
              rowCount: res.data.rowCount,
            },
            Datas: [...res.data.data],
            FilterInfo: {
              ...stateFilter,
            },
          });
        }
      })
      .catch(function (error) {
        alert(error);
      });
  };
  const ChangePage = (PageNumber) => {
    if (stateDatas.FilterInfo.SelectSite.length === 0) {
      alert("Please Choose at least one Site");
      return false;
    }
    let Data = {
      FuncKey: stateDatas.FilterInfo.Funckey,
      PageSize: parseInt(10),
      PageNumber: parseInt(PageNumber),
    };
    api
      .post("/api/ReasonManagement/GetReasonManagementData", Data)
      .then(function (res) {
        if (res.data.errorCode === 0) {
          setDatas({
            ...stateDatas,
            PageInfo: {
              ...stateDatas.PageInfo,
              pageNumber: PageNumber,
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
  const Edit = (props) => {
    console.log(props)
    seteditData({
      ...editData,
      Id: props.id,
      FuncKey: props.funcKey,
      ReasonId: props.reasonId,
      Description: props.description,
      Creator: props.creator,
      CreateTime: props.createTime,
      Updator: props.updator,
      UpdateTime: props.updateTime,
    });
    setstate(1);
  };
  const Edit_Change=(v, dataColumn)=>{
    seteditData({
      ...editData,
      [dataColumn]:v
    })
  }
  const Edit_Submit=()=>{
    let Data={
      Id:editData.Id,
      Description:editData.Description
    }
  }
  if (state === 0) {
    return (
      <div className="container-fluid">
        <h4 className="page-name">Draw Prize Report</h4>
        <Filter
          onChange={Filter_OnChange}
          ClickSubmit={Filter_Submit}
          SiteList={siteList}
          Funckey={funckey}
        />
        {stateDatas.Datas === null ? null : (
          <Result
            {...stateDatas}
            Edit={Edit}
            ChangePage={ChangePage}
            Submit={Edit_Submit}
          />
        )}
      </div>
    );
  } else if (state === 1) {
    return (
      <div>
        <Detail 
          EditData={editData}
          Change={Edit_Change}
        />
      </div>
    );
  }
  return <div></div>;
};
export default App;

if (process.env.NODE_ENV === "production") {
  ReactDOM.render(<App />, document.getElementById("content"));
}
