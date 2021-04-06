import React, { useEffect, useRef, useReducer, useState } from "react";
import ReactDOM from "react-dom";
import WebAPI, { api } from "../../../module/WebAPI";
import moment from "moment";
import Filter from "./Filter";
import Result from "./Result";
import Detail from "./Detail";
import Create from "./Create";
import qs from "qs";

const App = () => {
  const [state, setstate] = useState(0);
  const [siteList, setSiteList] = useState([]);
  const [stateFilter, setFilter] = useState({
    SelectSite: [],
    StartDate: moment().add(-10, "days").format("l"),
    EndDate: moment().format("l"),
    Lang: 0,
  });
  const [stateDatas, setDatas] = useState({
    PageInfo: null,
    Datas: null,
    FilterInfo: null,
  });
  const [alterDatas, setalterDatas] = useState({
    Date: "",
    SiteName: "",
    LangId: "",
    Title: "",
    Link: "",
    VideoId: 0,
    SiteId: 0,
  });
  const [langDatas, setlangDatas] = useState(null);
  const [createStatus, setcreateStatus] = useState({
    createSite: 0,
    createDate: moment().format("l"),
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
    if (v !== "All") return false;
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
  const Detail_Filter_OnChange = (v, dataColumn) => {
    setalterDatas({
      ...alterDatas,
      [dataColumn]: v,
    });
  };
  const Detail_Update = () => {
    let Data = {
      Date: moment(alterDatas.Date).format("YYYY-MM-DD"),
      Title: alterDatas.Title,
      Link: alterDatas.Link,
      VideoId: alterDatas.VideoId,
    };
    api
      .post("/api/LatestHighlightVideoManagement/SetLatestHighlightVideo", Data)
      .then(function (res) {
        if (res.data.errorCode === 0) {
          re_Submit();
          setstate(0);
        } else {
          alert(res.data.message ?? "failed to get data.");
        }
      })
      .catch(function (error) {
        alert(error);
      });
  };

  const Filter_Submit = () => {
    if (stateFilter.SelectSite.length === 0) {
      alert("Please Choose at least one Site");
      return false;
    }
    let Data = {
      SelectSite: stateFilter.SelectSite,
      PageNumber: 1,
      StartDate: moment(stateFilter.StartDate).format("YYYY-MM-DD"),
      EndDate: moment(stateFilter.EndDate).format("YYYY-MM-DD"),
      Lang: parseInt(stateFilter.Lang),
    };
    api
      .post(
        "/api/LatestHighlightVideoManagement/GetLatestHighlightVideoData",
        Data
      )
      .then(function (res) {
        if (res.data.errorCode === 0) {
          console.log(res.data.data);
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
        } else {
          alert(res.data.message ?? "failed to get data.");
        }
      })
      .catch(function (error) {
        alert(error);
      });
  };
  const Result_ChangePage = (PageNumber) => {
    if (stateDatas.FilterInfo.SelectSite.length === 0) {
      alert("Please Choose at least one Site");
      return false;
    }
    let Data = {
      SelectSite: stateDatas.FilterInfo.SelectSite,
      StartDate: moment(stateDatas.FilterInfo.StartDate).format("YYYY-MM-DD"),
      EndDate: moment(stateDatas.FilterInfo.EndDate).format("YYYY-MM-DD"),
      PageNumber: PageNumber,
      Lang: parseInt(stateDatas.FilterInfo.Lang),
    };
    api
      .post(
        "/api/LatestHighlightVideoManagement/GetLatestHighlightVideoData",
        Data
      )
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
  const re_Submit = () => {
    let Data = {
      SelectSite: stateDatas.FilterInfo.SelectSite,
      StartDate: moment(stateDatas.FilterInfo.StartDate).format("YYYY-MM-DD"),
      EndDate: moment(stateDatas.FilterInfo.EndDate).format("YYYY-MM-DD"),
      PageNumber: stateDatas.PageInfo.pageNumber,
      Lang: parseInt(stateDatas.FilterInfo.Lang),
    };
    api
      .post(
        "/api/LatestHighlightVideoManagement/GetLatestHighlightVideoData",
        Data
      )
      .then(function (res) {
        if (res.data.errorCode === 0) {
          setDatas({
            ...stateDatas,
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
  const GetSiteLang = (props) => {
    setcreateStatus({
      ...createStatus,
      createSite: props,
    });
    let Data = {
      SiteId: parseInt(props),
    };
    api
      .post("/api/LatestHighlightVideoManagement/GetSiteLangData", Data)
      .then(function (res) {
        if (res.data.errorCode === 0) {
          setlangDatas(res.data.data);
        } else {
          alert(res.data.message ?? "failed to get data.");
        }
      })
      .catch(function (error) {
        alert(error);
      });
  };
  const Alter = (props) => {
    setalterDatas({
      ...alterDatas,
      Date: props.date,
      SiteName: props.siteName,
      LangId: props.langId,
      Link: props.link,
      Title: props.title,
      VideoId: props.videoId,
      SiteId: props.siteId,
    });
    setstate(1);
  };
  const Back = () => {
    setstate(0);
  };
  const CreateHighlightVideo = () => {
    setstate(2);
  };
  const SetCreateData = (v, dataColumn, idx) => {
    var stateCopy = [...langDatas];
    stateCopy[idx][dataColumn] = v;
    setlangDatas(stateCopy);
  };
  const ChangeCreateDate = (props) => {
    setcreateStatus({
      ...createStatus,
      createDate: props,
    });
  };
  const CreateSubmit = () => {
    let Data = {
      SiteId: parseInt(createStatus.createSite),
      Date: moment(createStatus.createDate).format("YYYY-MM-DD"),
      Detail: JSON.stringify(langDatas),
    };
    api
      .post(
        "/api/LatestHighlightVideoManagement/InsertLatestHighlightVideo",
        Data
      )
      .then(function (res) {
        if (res.data.errorCode === 0) {
          re_Submit();
          CreateBack();
        } else {
          alert(res.data.message ?? "failed to get data.");
        }
      })
      .catch(function (error) {
        alert(error);
      });
  };
  const CreateBack = () => {
    setstate(0);
    setcreateStatus({
      createSite: 0,
      createDate: moment().format("l"),
    });
    setlangDatas(null);
  };
  if (state === 0) {
    return (
      <div className="container-fluid">
        <h4 className="page-name">Latest Highlight Video Management</h4>
        <Filter
          filterInfo={stateFilter}
          SiteList={siteList}
          onChange={Filter_OnChange}
          onChangeCheckBox={Filter_CheckBox_OnChange}
          onChangeAllCheckBox={Filter_CheckBox_CheckAll}
          ClickSubmit={Filter_Submit}
        />
        <button
          className="btn btn-add"
          style={{ marginBottom: "20px" }}
          onClick={CreateHighlightVideo}
        >
          Create Highlight Video
        </button>
        {stateDatas.Datas === null ? null : (
          <Result
            {...stateDatas}
            ChangePage={Result_ChangePage}
            Alter={Alter}
          />
        )}
      </div>
    );
  } else if (state === 1) {
    return (
      <div>
        <Detail
          AlterDatas={alterDatas}
          FiterOnChange={Detail_Filter_OnChange}
          Update={Detail_Update}
          Back={Back}
        />
      </div>
    );
  } else {
    return (
      <div>
        <Create
          SiteList={siteList}
          //CreateSite={createSite}
          SetcreateSite={GetSiteLang}
          LangDatas={langDatas}
          SetCreateData={SetCreateData}
          CreateStatus={createStatus}
          ChangeCreateDate={ChangeCreateDate}
          CreateSubmit={CreateSubmit}
          Back={CreateBack}
        />
      </div>
    );
  }
};

export default App;

if (process.env.NODE_ENV === "production") {
  ReactDOM.render(<App />, document.getElementById("content"));
}
