import { number_format } from "../../commJS/common";
import moment from "moment-timezone";
import Pagination from "./Pagination";

const Result = (props) => {
  let rows =
    props.Datas.length === 0
      ? null
      : props.Datas.map((d, dIdx) => {
          return (
            <tr key={`result-${dIdx}`}>
              <td>{d.mainType == 1 ? "Add" : "Deduct"}</td>
              <td>{number_format(d.changeWithdrawalLimit)}</td>
              <td>{number_format(d.beforeWithdrawalLimit)}</td>
              <td>{number_format(d.afterWithdrawalLimit)}</td>
              <td>{d.memo}</td>
              <td>{d.refTransId}</td>
              <td>{moment(d.createTime).format("l LTS")}</td>
            </tr>
          );
        });
  return (
    <>
      <div className="table-responsive-md table">
        <span class="label">WithdrawalLimitLogList</span>
        <table className="table table-striped">
          <thead>
            <tr>
              <th scope="col">MainType</th>
              <th scope="col">ChangeWithdrawalLimit</th>
              <th scope="col">BeforeWithdrawalLimit</th>
              <th scope="col">AfterWithdrawalLimit</th>
              <th scope="col">Memo</th>
              <th scope="col">RefTransId</th>
              <th scope="col">CreateTime</th>
            </tr>
          </thead>
          {rows}
        </table>
        <Pagination
          Data={{
            totalPages: props.PageInfo.totalPages,
            pageNumber: props.PageInfo.pageNumber,
            rowCount: props.PageInfo.rowCount,
          }}
          ChangePage={props.ChangePage}
        />
      </div>
    </>
  );
};

export default Result;
