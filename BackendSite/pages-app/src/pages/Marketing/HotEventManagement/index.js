import React, { useEffect, useRef, useReducer, useState } from "react";
import Filter from "./Filter";
import WebAPI from "../../../module/WebAPI";

const App = () => {
  const [siteList, setSiteList] = useState();
  const [filter, setfilter] = useState({
    SiteId: 0,
    CurrencyId: 0,
    Status: 0,
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
  const changeFilter = (value, type) => {
    setfilter({
        ...filter,
        [type]:value
    })

  };
  return (
    <div>
      <h4 className="detail">Hot Event Management</h4>
      {siteList == null ? null : (
        <Filter Site={siteList} ChangeFilter={changeFilter} Filter={filter}/>
      )}
    </div>
  );
};

export default App;

if (process.env.NODE_ENV === "production") {
  ReactDOM.render(<App />, document.getElementById("content"));
}
