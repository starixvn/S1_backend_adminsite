import React, { useEffect, useRef, useReducer, useState } from "react";
import ReactDOM from "react-dom";
import WebAPI from "../../../module/WebAPI";
import moment from "moment";
import { api } from "../../../module/WebAPI";
import Filter from "./Filter";

const App = () => {
  const [state, setstate] = useState(0);
  const [siteList, setSiteList] = useState([]);
  const [funckey, setfunckey] = useState(null);
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
        setfunckey(res.data.data);
      } else {
        alert(res.data.message ?? "failed to get data.");
      }
    });
  }, []);
  if (state === 0) {
    <div className="container-fluid">
      <h4 className="page-name">Draw Prize Report</h4>
      <Filter
        // filterInfo={stateFilter}
        // onChange={Filter_OnChange}
        // onChangeCheckBox={Filter_CheckBox_OnChange}
        // onChangeAllCheckBox={Filter_CheckBox_CheckAll}
        // ClickSubmit={Filter_Submit}
        SiteList={siteList}
        Funckey={funckey}
      />
    </div>;
  }
  return <div></div>;
};
export default App;

if (process.env.NODE_ENV === "production") {
  ReactDOM.render(<App />, document.getElementById("content"));
}
