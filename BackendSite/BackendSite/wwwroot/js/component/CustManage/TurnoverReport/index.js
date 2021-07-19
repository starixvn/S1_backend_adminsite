import Filter from "./Filter";
import Result from "./Result";
import { getUrlParameter } from "../../commJS/common";

const Panel = () => {
  const [stateDatas, setDatas] = React.useState();
  let CustId = getUrlParameter("custId");
  const onSubmit = (filterDatas) => {
    console.log(filterDatas.GameTypes);
    axios
      .post("/api/CustManage/GetCustTurnOverReport", {
        CustId: parseInt(CustId),
        StartTime: moment(new Date(filterDatas.StartTime)).unix(),
        EndTime: moment(new Date(filterDatas.EndTime)).unix(),
        GameTypes: filterDatas.GameTypes,
      })
      .then((res) => {
        let result = res.data;
        if (result.errorCode == 0) {
          setDatas(result.data);
        } else {
          alert(result.message);
        }
      })
      .catch((error) => {
        alert(error);
      });
  };

  return (
    <>
      <Filter ClickSubmit={onSubmit} />
      {stateDatas === undefined ? null : <Result Datas={stateDatas} />}
    </>
  );
};

export default Panel;
