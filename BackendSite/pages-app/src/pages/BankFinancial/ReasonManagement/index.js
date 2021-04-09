import React, { useEffect, useRef, useReducer, useState } from "react";
import ReactDOM from "react-dom";
import WebAPI from "../../../module/WebAPI";
import moment from "moment";
import { api } from "../../../module/WebAPI";
import Filter from "./Filter";
import Result from "./Result";
import Detail from "./Detail";
import Create from "./Create";

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
  const [insertData, setinsertData] = useState({
    OldFuncKey: "0",
    NewFuncKey: "",
    Description: "",
    FuncKeyType: 0,
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
  }, [state]);
  const Filter_OnChange = (v, dataColumn) => {
    setFilter({
      ...stateFilter,
      [dataColumn]: v,
    });
  };
  const Create_OnChange = (v, dataColumn) => {
    console.log(v, dataColumn);
    setinsertData({
      ...insertData,
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
              totalPages: res.data.totalPages,
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
  const Edit = (props) => {
    console.log(props);
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
  const Edit_Change = (v, dataColumn) => {
    seteditData({
      ...editData,
      [dataColumn]: v,
    });
  };
  const Edit_Submit = () => {
    let Data = {
      Id: editData.Id,
      Description: editData.Description,
    };
    api
      .post("/api/ReasonManagement/SetReasonManagement", Data)
      .then(function (res) {
        if (res.data.errorCode === 0) {
          Resubmit();
        } else {
          alert(res.data.message ?? "failed to get data.");
        }
      })
      .catch(function (error) {
        alert(error);
      });
  };
  const Resubmit = () => {
    let Data = {
      FuncKey: stateDatas.FilterInfo.Funckey,
      PageSize: parseInt(10),
      PageNumber: parseInt(stateDatas.PageInfo.pageNumber),
    };
    console.log(Data);
    api
      .post("/api/ReasonManagement/GetReasonManagementData", Data)
      .then(function (res) {
        if (res.data.errorCode === 0) {
          setDatas({
            ...stateDatas,
            PageInfo: {
              ...stateDatas.PageInfo,
              totalPages: res.data.totalPages,
              rowCount: res.data.rowCount,
            },
            Datas: [...res.data.data],
          });
          setstate(0);
        } else {
          alert(res.data.message ?? "failed to get data.");
        }
      })
      .catch(function (error) {
        alert(error);
      });
  };
  const Back = () => {
    setstate(0);
  };
  const CreateOne = () => {
    setstate(2);
  };
  const Create_Submit = () => {
    console.log(111);
    if (insertData.FuncKeyType === 0 && insertData.OldFuncKey === "0") {
      alert("Please Choose at least one FuncKey");
      return false;
    }
    if (
      insertData.FuncKeyType === 1 &&
      insertData.NewFuncKey.replace(/(^\s*)|(\s*$)/g, "") === ""
    ) {
      alert("Please Fill in FuncKey");
      return false;
    }
    let Data = {
      FuncKey:
        insertData.FuncKeyType === 0
          ? insertData.OldFuncKey
          : insertData.NewFuncKey,
      Description: insertData.Description,
    };
    api
      .post("/api/ReasonManagement/InsertReasonManagement", Data)
      .then(function (res) {
        if (res.data.errorCode === 0) {
          ResetinsertData();
        } else {
          alert(res.data.message ?? "failed to get data.");
        }
      })
      .catch(function (error) {
        alert(error);
      });
  };
  const ResetinsertData = ()=>{
    setinsertData({
      ...insertData,
      OldFuncKey: "0",
      NewFuncKey: "",
      Description: "",
      FuncKeyType: 0,
    })
    console.log(stateDatas);
    if(stateDatas.Datas !== null){
      Resubmit();
    }   
    setstate(0);
  }
  if (state === 0) {
    return (
      <div className="container-fluid">
        <h4 className="page-name">Reason Management</h4>
        <Filter
          onChange={Filter_OnChange}
          ClickSubmit={Filter_Submit}
          SiteList={siteList}
          Funckey={funckey}
          stateFilter={stateFilter}
        />
        {stateDatas.Datas === null ? null : (
          <Result
            {...stateDatas}
            Edit={Edit}
            ChangePage={ChangePage}
            Submit={Edit_Submit}
          />
        )}
        <div className="row-flex">
          <div className="button-group">
            <button
              className="btn btn-search"
              type="submit"
              onClick={CreateOne}
              style={{
                marginTop: "20px",
                marginBottom: "40px",
                marginRight: "10px",
              }}
            >
              Create
            </button>
          </div>
        </div>
      </div>
    );
  } else if (state === 1) {
    return (
      <div>
        <Detail
          EditData={editData}
          Change={Edit_Change}
          Submit={Edit_Submit}
          Back={Back}
        />
      </div>
    );
  } else {
    return (
      <div>
        <Create
          insertData={insertData}
          onChange={Create_OnChange}
          Funckey={funckey}
          Back={Back}
          Submit={Create_Submit}
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
